package app

import (
	"encoding/json"
	"errors"
	"fmt"
	"html/template"
	"net/http"
	"strconv"
	"strings"
)

type Server struct {
	store         *FileStore
	sessionSecret []byte
	templates     *template.Template
}

type pageData struct {
	Title   string
	User    *User
	Error   string
	Success string
	Polls   []Poll
	Poll    Poll
	Result  PollResult
	Voted   bool
}

func NewServer(store *FileStore, sessionSecret []byte) *Server {
	return &Server{
		store:         store,
		sessionSecret: sessionSecret,
		templates:     template.Must(template.New("pages").Parse(pagesHTML)),
	}
}

func (s *Server) Routes() http.Handler {
	mux := http.NewServeMux()
	mux.Handle("/static/", http.StripPrefix("/static/", http.FileServer(http.Dir("web/static"))))

	mux.HandleFunc("/", s.handleHome)
	mux.HandleFunc("/register", s.handleRegister)
	mux.HandleFunc("/login", s.handleLogin)
	mux.HandleFunc("/logout", s.handleLogout)
	mux.HandleFunc("/polls", s.requireAuth(s.handlePolls))
	mux.HandleFunc("/polls/", s.handlePollsSubroutes)
	mux.HandleFunc("/p/", s.handlePublicPoll)

	mux.HandleFunc("/api/auth/register", s.apiRegister)
	mux.HandleFunc("/api/auth/login", s.apiLogin)
	mux.HandleFunc("/api/polls", s.requireAPIAuth(s.apiPolls))
	mux.HandleFunc("/api/polls/", s.apiPollSubroutes)
	return mux
}

func (s *Server) currentUser(r *http.Request) (*User, bool) {
	cookie, err := r.Cookie(sessionCookieName)
	if err != nil {
		return nil, false
	}
	userID, ok := parseSessionValue(cookie.Value, s.sessionSecret)
	if !ok {
		return nil, false
	}
	user, ok := s.store.UserByID(userID)
	if !ok {
		return nil, false
	}
	return &user, true
}

func (s *Server) requireAuth(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if _, ok := s.currentUser(r); !ok {
			http.Redirect(w, r, "/login", http.StatusSeeOther)
			return
		}
		next(w, r)
	}
}

func (s *Server) requireAPIAuth(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		if _, ok := s.currentUser(r); !ok {
			writeJSON(w, http.StatusUnauthorized, map[string]string{"error": "authorization required"})
			return
		}
		next(w, r)
	}
}

func (s *Server) render(w http.ResponseWriter, r *http.Request, name string, data pageData) {
	if user, ok := s.currentUser(r); ok {
		data.User = user
	}
	w.Header().Set("Content-Type", "text/html; charset=utf-8")
	if err := s.templates.ExecuteTemplate(w, name, data); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
	}
}

func (s *Server) handleHome(w http.ResponseWriter, r *http.Request) {
	if r.URL.Path != "/" {
		http.NotFound(w, r)
		return
	}
	if _, ok := s.currentUser(r); ok {
		http.Redirect(w, r, "/polls", http.StatusSeeOther)
		return
	}
	s.render(w, r, "home", pageData{Title: "Опросы и голосования"})
}

func (s *Server) handleRegister(w http.ResponseWriter, r *http.Request) {
	if r.Method == http.MethodGet {
		s.render(w, r, "register", pageData{Title: "Регистрация"})
		return
	}
	if r.Method != http.MethodPost {
		http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
		return
	}
	name := strings.TrimSpace(r.FormValue("name"))
	email := strings.TrimSpace(strings.ToLower(r.FormValue("email")))
	password := r.FormValue("password")
	if name == "" || email == "" || len(password) < 6 {
		s.render(w, r, "register", pageData{Title: "Регистрация", Error: "Заполните имя, email и пароль длиной от 6 символов."})
		return
	}
	salt := randomToken(16)
	user, err := s.store.CreateUser(name, email, salt, hashPassword(password, salt))
	if errors.Is(err, ErrAlreadyExists) {
		s.render(w, r, "register", pageData{Title: "Регистрация", Error: "Пользователь с таким email уже существует."})
		return
	}
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	setSession(w, user.ID, s.sessionSecret)
	http.Redirect(w, r, "/polls", http.StatusSeeOther)
}

func (s *Server) handleLogin(w http.ResponseWriter, r *http.Request) {
	if r.Method == http.MethodGet {
		s.render(w, r, "login", pageData{Title: "Вход"})
		return
	}
	if r.Method != http.MethodPost {
		http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
		return
	}
	email := strings.TrimSpace(strings.ToLower(r.FormValue("email")))
	password := r.FormValue("password")
	user, ok := s.store.UserByEmail(email)
	if !ok || !secureCompare(user.PasswordHash, hashPassword(password, user.Salt)) {
		s.render(w, r, "login", pageData{Title: "Вход", Error: "Неверный email или пароль."})
		return
	}
	setSession(w, user.ID, s.sessionSecret)
	http.Redirect(w, r, "/polls", http.StatusSeeOther)
}

func (s *Server) handleLogout(w http.ResponseWriter, r *http.Request) {
	clearSession(w)
	http.Redirect(w, r, "/", http.StatusSeeOther)
}

func (s *Server) handlePolls(w http.ResponseWriter, r *http.Request) {
	user, _ := s.currentUser(r)
	switch r.Method {
	case http.MethodGet:
		polls := s.store.PollsByOwner(user.ID)
		s.render(w, r, "polls", pageData{Title: "Мои опросы", Polls: polls})
	case http.MethodPost:
		title := strings.TrimSpace(r.FormValue("title"))
		description := strings.TrimSpace(r.FormValue("description"))
		if title == "" {
			polls := s.store.PollsByOwner(user.ID)
			s.render(w, r, "polls", pageData{Title: "Мои опросы", Polls: polls, Error: "Введите название опроса."})
			return
		}
		poll, err := s.store.CreatePoll(user.ID, title, description)
		if err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}
		http.Redirect(w, r, fmt.Sprintf("/polls/%d", poll.ID), http.StatusSeeOther)
	default:
		http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
	}
}

func (s *Server) handlePollsSubroutes(w http.ResponseWriter, r *http.Request) {
	user, ok := s.currentUser(r)
	if !ok {
		http.Redirect(w, r, "/login", http.StatusSeeOther)
		return
	}

	parts := splitPath(strings.TrimPrefix(r.URL.Path, "/polls/"))
	if len(parts) == 0 {
		http.NotFound(w, r)
		return
	}
	pollID, err := strconv.Atoi(parts[0])
	if err != nil {
		http.NotFound(w, r)
		return
	}

	if len(parts) == 1 {
		poll, found := s.store.PollByID(pollID)
		if !found || poll.OwnerID != user.ID {
			http.NotFound(w, r)
			return
		}
		s.render(w, r, "poll", pageData{Title: poll.Title, Poll: poll})
		return
	}

	switch parts[1] {
	case "questions":
		if r.Method != http.MethodPost {
			http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
			return
		}
		text := strings.TrimSpace(r.FormValue("text"))
		options := cleanLines(r.FormValue("options"))
		if text == "" || len(options) < 2 {
			poll, _ := s.store.PollByID(pollID)
			s.render(w, r, "poll", pageData{Title: poll.Title, Poll: poll, Error: "Введите вопрос и минимум два варианта ответа."})
			return
		}
		if _, err := s.store.AddQuestion(user.ID, pollID, text, options); err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}
		http.Redirect(w, r, fmt.Sprintf("/polls/%d", pollID), http.StatusSeeOther)
	case "publish":
		if r.Method != http.MethodPost {
			http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
			return
		}
		if err := s.store.PublishPoll(user.ID, pollID); err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}
		http.Redirect(w, r, fmt.Sprintf("/polls/%d", pollID), http.StatusSeeOther)
	case "delete":
		if r.Method != http.MethodPost {
			http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
			return
		}
		if err := s.store.DeletePoll(user.ID, pollID); err != nil {
			http.Error(w, err.Error(), http.StatusInternalServerError)
			return
		}
		http.Redirect(w, r, "/polls", http.StatusSeeOther)
	case "results":
		result, err := s.store.Results(pollID)
		if err != nil || result.Poll.OwnerID != user.ID {
			http.NotFound(w, r)
			return
		}
		s.render(w, r, "results", pageData{Title: "Результаты", Result: result})
	default:
		http.NotFound(w, r)
	}
}

func (s *Server) handlePublicPoll(w http.ResponseWriter, r *http.Request) {
	parts := splitPath(strings.TrimPrefix(r.URL.Path, "/p/"))
	if len(parts) != 1 {
		http.NotFound(w, r)
		return
	}
	pollID, err := strconv.Atoi(parts[0])
	if err != nil {
		http.NotFound(w, r)
		return
	}
	poll, found := s.store.PollByID(pollID)
	if !found || !poll.Published {
		http.NotFound(w, r)
		return
	}

	userID := 0
	if user, ok := s.currentUser(r); ok {
		userID = user.ID
	}
	voterToken := ""
	if cookie, err := r.Cookie(voterCookieName(pollID)); err == nil {
		voterToken = cookie.Value
	}
	voted := s.store.HasResponse(pollID, voterToken, userID)

	if r.Method == http.MethodGet {
		s.render(w, r, "publicPoll", pageData{Title: poll.Title, Poll: poll, Voted: voted})
		return
	}
	if r.Method != http.MethodPost {
		http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
		return
	}
	if voted {
		s.render(w, r, "publicPoll", pageData{Title: poll.Title, Poll: poll, Voted: true, Error: "Вы уже прошли этот опрос."})
		return
	}

	answers := make([]Answer, 0, len(poll.Questions))
	for _, question := range poll.Questions {
		optionID, err := strconv.Atoi(r.FormValue(fmt.Sprintf("q_%d", question.ID)))
		if err != nil || !optionBelongsToQuestion(question, optionID) {
			s.render(w, r, "publicPoll", pageData{Title: poll.Title, Poll: poll, Error: "Ответьте на все вопросы."})
			return
		}
		answers = append(answers, Answer{QuestionID: question.ID, OptionID: optionID})
	}

	if voterToken == "" {
		voterToken = randomToken(20)
	}
	if err := s.store.SaveResponse(pollID, userID, voterToken, answers); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	http.SetCookie(w, &http.Cookie{
		Name:     voterCookieName(pollID),
		Value:    voterToken,
		Path:     "/",
		HttpOnly: true,
		SameSite: http.SameSiteLaxMode,
		MaxAge:   365 * 24 * 60 * 60,
	})
	s.render(w, r, "publicPoll", pageData{Title: poll.Title, Poll: poll, Voted: true, Success: "Спасибо, ваш голос сохранен."})
}

func (s *Server) apiRegister(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		writeJSON(w, http.StatusMethodNotAllowed, map[string]string{"error": "method not allowed"})
		return
	}
	var input struct {
		Name     string `json:"name"`
		Email    string `json:"email"`
		Password string `json:"password"`
	}
	if err := json.NewDecoder(r.Body).Decode(&input); err != nil {
		writeJSON(w, http.StatusBadRequest, map[string]string{"error": "invalid json"})
		return
	}
	salt := randomToken(16)
	user, err := s.store.CreateUser(strings.TrimSpace(input.Name), strings.ToLower(strings.TrimSpace(input.Email)), salt, hashPassword(input.Password, salt))
	if err != nil {
		writeJSON(w, http.StatusBadRequest, map[string]string{"error": err.Error()})
		return
	}
	setSession(w, user.ID, s.sessionSecret)
	user.PasswordHash = ""
	user.Salt = ""
	writeJSON(w, http.StatusCreated, user)
}

func (s *Server) apiLogin(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		writeJSON(w, http.StatusMethodNotAllowed, map[string]string{"error": "method not allowed"})
		return
	}
	var input struct {
		Email    string `json:"email"`
		Password string `json:"password"`
	}
	if err := json.NewDecoder(r.Body).Decode(&input); err != nil {
		writeJSON(w, http.StatusBadRequest, map[string]string{"error": "invalid json"})
		return
	}
	user, ok := s.store.UserByEmail(strings.ToLower(strings.TrimSpace(input.Email)))
	if !ok || !secureCompare(user.PasswordHash, hashPassword(input.Password, user.Salt)) {
		writeJSON(w, http.StatusUnauthorized, map[string]string{"error": "invalid credentials"})
		return
	}
	setSession(w, user.ID, s.sessionSecret)
	user.PasswordHash = ""
	user.Salt = ""
	writeJSON(w, http.StatusOK, user)
}

func (s *Server) apiPolls(w http.ResponseWriter, r *http.Request) {
	user, _ := s.currentUser(r)
	switch r.Method {
	case http.MethodGet:
		writeJSON(w, http.StatusOK, s.store.PollsByOwner(user.ID))
	case http.MethodPost:
		var input struct {
			Title       string `json:"title"`
			Description string `json:"description"`
		}
		if err := json.NewDecoder(r.Body).Decode(&input); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]string{"error": "invalid json"})
			return
		}
		poll, err := s.store.CreatePoll(user.ID, strings.TrimSpace(input.Title), strings.TrimSpace(input.Description))
		if err != nil {
			writeJSON(w, http.StatusInternalServerError, map[string]string{"error": err.Error()})
			return
		}
		writeJSON(w, http.StatusCreated, poll)
	default:
		writeJSON(w, http.StatusMethodNotAllowed, map[string]string{"error": "method not allowed"})
	}
}

func (s *Server) apiPollSubroutes(w http.ResponseWriter, r *http.Request) {
	user, ok := s.currentUser(r)
	if !ok {
		writeJSON(w, http.StatusUnauthorized, map[string]string{"error": "authorization required"})
		return
	}
	parts := splitPath(strings.TrimPrefix(r.URL.Path, "/api/polls/"))
	if len(parts) < 2 {
		writeJSON(w, http.StatusNotFound, map[string]string{"error": "not found"})
		return
	}
	pollID, err := strconv.Atoi(parts[0])
	if err != nil {
		writeJSON(w, http.StatusNotFound, map[string]string{"error": "not found"})
		return
	}
	switch parts[1] {
	case "questions":
		var input struct {
			Text    string   `json:"text"`
			Options []string `json:"options"`
		}
		if err := json.NewDecoder(r.Body).Decode(&input); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]string{"error": "invalid json"})
			return
		}
		poll, err := s.store.AddQuestion(user.ID, pollID, strings.TrimSpace(input.Text), input.Options)
		if err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]string{"error": err.Error()})
			return
		}
		writeJSON(w, http.StatusCreated, poll)
	case "publish":
		if err := s.store.PublishPoll(user.ID, pollID); err != nil {
			writeJSON(w, http.StatusBadRequest, map[string]string{"error": err.Error()})
			return
		}
		writeJSON(w, http.StatusOK, map[string]bool{"published": true})
	case "results":
		result, err := s.store.Results(pollID)
		if err != nil || result.Poll.OwnerID != user.ID {
			writeJSON(w, http.StatusNotFound, map[string]string{"error": "not found"})
			return
		}
		writeJSON(w, http.StatusOK, result)
	default:
		writeJSON(w, http.StatusNotFound, map[string]string{"error": "not found"})
	}
}

func cleanLines(raw string) []string {
	lines := strings.Split(raw, "\n")
	result := make([]string, 0, len(lines))
	for _, line := range lines {
		line = strings.TrimSpace(line)
		if line != "" {
			result = append(result, line)
		}
	}
	return result
}

func splitPath(path string) []string {
	parts := strings.Split(strings.Trim(path, "/"), "/")
	result := make([]string, 0, len(parts))
	for _, part := range parts {
		if part != "" {
			result = append(result, part)
		}
	}
	return result
}

func optionBelongsToQuestion(question Question, optionID int) bool {
	for _, option := range question.Options {
		if option.ID == optionID {
			return true
		}
	}
	return false
}

func writeJSON(w http.ResponseWriter, status int, value any) {
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	w.WriteHeader(status)
	_ = json.NewEncoder(w).Encode(value)
}
