package app

import (
	"encoding/json"
	"errors"
	"os"
	"path/filepath"
	"sync"
	"time"
)

var (
	ErrNotFound      = errors.New("not found")
	ErrAlreadyExists = errors.New("already exists")
	ErrForbidden     = errors.New("forbidden")
)

type FileStore struct {
	path string
	mu   sync.Mutex
	data storeData
}

type storeData struct {
	NextUserID     int        `json:"next_user_id"`
	NextPollID     int        `json:"next_poll_id"`
	NextQuestionID int        `json:"next_question_id"`
	NextOptionID   int        `json:"next_option_id"`
	NextResponseID int        `json:"next_response_id"`
	Users          []User     `json:"users"`
	Polls          []Poll     `json:"polls"`
	Responses      []Response `json:"responses"`
}

func NewFileStore(path string) (*FileStore, error) {
	store := &FileStore{path: path}
	if err := store.load(); err != nil {
		return nil, err
	}
	return store, nil
}

func (s *FileStore) load() error {
	s.mu.Lock()
	defer s.mu.Unlock()

	s.data = storeData{
		NextUserID:     1,
		NextPollID:     1,
		NextQuestionID: 1,
		NextOptionID:   1,
		NextResponseID: 1,
	}

	file, err := os.Open(s.path)
	if errors.Is(err, os.ErrNotExist) {
		return s.saveLocked()
	}
	if err != nil {
		return err
	}
	defer file.Close()

	if err := json.NewDecoder(file).Decode(&s.data); err != nil {
		return err
	}
	s.normalizeCountersLocked()
	return nil
}

func (s *FileStore) saveLocked() error {
	if err := os.MkdirAll(filepath.Dir(s.path), 0755); err != nil {
		return err
	}
	temp := s.path + ".tmp"
	file, err := os.Create(temp)
	if err != nil {
		return err
	}
	encoder := json.NewEncoder(file)
	encoder.SetIndent("", "  ")
	if err := encoder.Encode(s.data); err != nil {
		file.Close()
		return err
	}
	if err := file.Close(); err != nil {
		return err
	}
	return os.Rename(temp, s.path)
}

func (s *FileStore) normalizeCountersLocked() {
	if s.data.NextUserID < 1 {
		s.data.NextUserID = 1
	}
	if s.data.NextPollID < 1 {
		s.data.NextPollID = 1
	}
	if s.data.NextQuestionID < 1 {
		s.data.NextQuestionID = 1
	}
	if s.data.NextOptionID < 1 {
		s.data.NextOptionID = 1
	}
	if s.data.NextResponseID < 1 {
		s.data.NextResponseID = 1
	}
}

func (s *FileStore) CreateUser(name, email, salt, passwordHash string) (User, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	for _, user := range s.data.Users {
		if user.Email == email {
			return User{}, ErrAlreadyExists
		}
	}

	user := User{
		ID:           s.data.NextUserID,
		Name:         name,
		Email:        email,
		Salt:         salt,
		PasswordHash: passwordHash,
		CreatedAt:    time.Now(),
	}
	s.data.NextUserID++
	s.data.Users = append(s.data.Users, user)
	return user, s.saveLocked()
}

func (s *FileStore) UserByEmail(email string) (User, bool) {
	s.mu.Lock()
	defer s.mu.Unlock()
	for _, user := range s.data.Users {
		if user.Email == email {
			return user, true
		}
	}
	return User{}, false
}

func (s *FileStore) UserByID(id int) (User, bool) {
	s.mu.Lock()
	defer s.mu.Unlock()
	for _, user := range s.data.Users {
		if user.ID == id {
			return user, true
		}
	}
	return User{}, false
}

func (s *FileStore) CreatePoll(ownerID int, title, description string) (Poll, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	now := time.Now()
	poll := Poll{
		ID:          s.data.NextPollID,
		OwnerID:     ownerID,
		Title:       title,
		Description: description,
		CreatedAt:   now,
		UpdatedAt:   now,
	}
	s.data.NextPollID++
	s.data.Polls = append(s.data.Polls, poll)
	return poll, s.saveLocked()
}

func (s *FileStore) PollsByOwner(ownerID int) []Poll {
	s.mu.Lock()
	defer s.mu.Unlock()
	polls := make([]Poll, 0)
	for _, poll := range s.data.Polls {
		if poll.OwnerID == ownerID {
			polls = append(polls, poll)
		}
	}
	return polls
}

func (s *FileStore) PollByID(id int) (Poll, bool) {
	s.mu.Lock()
	defer s.mu.Unlock()
	for _, poll := range s.data.Polls {
		if poll.ID == id {
			return poll, true
		}
	}
	return Poll{}, false
}

func (s *FileStore) AddQuestion(ownerID, pollID int, text string, options []string) (Poll, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	for i := range s.data.Polls {
		if s.data.Polls[i].ID != pollID {
			continue
		}
		if s.data.Polls[i].OwnerID != ownerID {
			return Poll{}, ErrForbidden
		}

		question := Question{ID: s.data.NextQuestionID, Text: text}
		s.data.NextQuestionID++
		for _, optionText := range options {
			question.Options = append(question.Options, Option{
				ID:   s.data.NextOptionID,
				Text: optionText,
			})
			s.data.NextOptionID++
		}
		s.data.Polls[i].Questions = append(s.data.Polls[i].Questions, question)
		s.data.Polls[i].UpdatedAt = time.Now()
		poll := s.data.Polls[i]
		return poll, s.saveLocked()
	}
	return Poll{}, ErrNotFound
}

func (s *FileStore) PublishPoll(ownerID, pollID int) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	for i := range s.data.Polls {
		if s.data.Polls[i].ID != pollID {
			continue
		}
		if s.data.Polls[i].OwnerID != ownerID {
			return ErrForbidden
		}
		s.data.Polls[i].Published = true
		s.data.Polls[i].UpdatedAt = time.Now()
		return s.saveLocked()
	}
	return ErrNotFound
}

func (s *FileStore) DeletePoll(ownerID, pollID int) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	for i := range s.data.Polls {
		if s.data.Polls[i].ID != pollID {
			continue
		}
		if s.data.Polls[i].OwnerID != ownerID {
			return ErrForbidden
		}
		s.data.Polls = append(s.data.Polls[:i], s.data.Polls[i+1:]...)
		filtered := s.data.Responses[:0]
		for _, response := range s.data.Responses {
			if response.PollID != pollID {
				filtered = append(filtered, response)
			}
		}
		s.data.Responses = filtered
		return s.saveLocked()
	}
	return ErrNotFound
}

func (s *FileStore) HasResponse(pollID int, voterToken string, userID int) bool {
	s.mu.Lock()
	defer s.mu.Unlock()
	for _, response := range s.data.Responses {
		if response.PollID != pollID {
			continue
		}
		if userID > 0 && response.UserID == userID {
			return true
		}
		if voterToken != "" && response.VoterToken == voterToken {
			return true
		}
	}
	return false
}

func (s *FileStore) SaveResponse(pollID, userID int, voterToken string, answers []Answer) error {
	s.mu.Lock()
	defer s.mu.Unlock()

	response := Response{
		ID:         s.data.NextResponseID,
		PollID:     pollID,
		UserID:     userID,
		VoterToken: voterToken,
		Answers:    answers,
		CreatedAt:  time.Now(),
	}
	s.data.NextResponseID++
	s.data.Responses = append(s.data.Responses, response)
	return s.saveLocked()
}

func (s *FileStore) Results(pollID int) (PollResult, error) {
	s.mu.Lock()
	defer s.mu.Unlock()

	var poll Poll
	found := false
	for _, item := range s.data.Polls {
		if item.ID == pollID {
			poll = item
			found = true
			break
		}
	}
	if !found {
		return PollResult{}, ErrNotFound
	}

	result := PollResult{Poll: poll}
	counts := make(map[int]int)
	for _, response := range s.data.Responses {
		if response.PollID != pollID {
			continue
		}
		result.Total++
		for _, answer := range response.Answers {
			counts[answer.OptionID]++
		}
	}

	for _, question := range poll.Questions {
		questionResult := QuestionResult{ID: question.ID, Text: question.Text}
		for _, option := range question.Options {
			count := counts[option.ID]
			percent := 0
			if result.Total > 0 {
				percent = count * 100 / result.Total
			}
			questionResult.Options = append(questionResult.Options, OptionResult{
				ID:      option.ID,
				Text:    option.Text,
				Count:   count,
				Percent: percent,
			})
		}
		result.Questions = append(result.Questions, questionResult)
	}
	return result, nil
}
