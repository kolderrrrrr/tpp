package app

import "time"

type User struct {
	ID           int       `json:"id"`
	Name         string    `json:"name"`
	Email        string    `json:"email"`
	PasswordHash string    `json:"password_hash"`
	Salt         string    `json:"salt"`
	CreatedAt    time.Time `json:"created_at"`
}

type Poll struct {
	ID          int        `json:"id"`
	OwnerID     int        `json:"owner_id"`
	Title       string     `json:"title"`
	Description string     `json:"description"`
	Published   bool       `json:"published"`
	Questions   []Question `json:"questions"`
	CreatedAt   time.Time  `json:"created_at"`
	UpdatedAt   time.Time  `json:"updated_at"`
}

type Question struct {
	ID      int      `json:"id"`
	Text    string   `json:"text"`
	Options []Option `json:"options"`
}

type Option struct {
	ID   int    `json:"id"`
	Text string `json:"text"`
}

type Response struct {
	ID         int       `json:"id"`
	PollID     int       `json:"poll_id"`
	UserID     int       `json:"user_id,omitempty"`
	VoterToken string    `json:"voter_token"`
	Answers    []Answer  `json:"answers"`
	CreatedAt  time.Time `json:"created_at"`
}

type Answer struct {
	QuestionID int `json:"question_id"`
	OptionID   int `json:"option_id"`
}

type PollResult struct {
	Poll      Poll             `json:"poll"`
	Total     int              `json:"total"`
	Questions []QuestionResult `json:"questions"`
}

type QuestionResult struct {
	ID      int            `json:"id"`
	Text    string         `json:"text"`
	Options []OptionResult `json:"options"`
}

type OptionResult struct {
	ID      int    `json:"id"`
	Text    string `json:"text"`
	Count   int    `json:"count"`
	Percent int    `json:"percent"`
}
