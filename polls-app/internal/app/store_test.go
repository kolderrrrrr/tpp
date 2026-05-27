package app

import (
	"path/filepath"
	"testing"
)

func TestPollWorkflow(t *testing.T) {
	store, err := NewFileStore(filepath.Join(t.TempDir(), "store.json"))
	if err != nil {
		t.Fatalf("create store: %v", err)
	}

	user, err := store.CreateUser("Test User", "test@example.com", "salt", "hash")
	if err != nil {
		t.Fatalf("create user: %v", err)
	}

	poll, err := store.CreatePoll(user.ID, "Favorite language", "Course project poll")
	if err != nil {
		t.Fatalf("create poll: %v", err)
	}

	poll, err = store.AddQuestion(user.ID, poll.ID, "Choose one", []string{"Go", "Python"})
	if err != nil {
		t.Fatalf("add question: %v", err)
	}

	if err := store.PublishPoll(user.ID, poll.ID); err != nil {
		t.Fatalf("publish poll: %v", err)
	}

	answer := Answer{
		QuestionID: poll.Questions[0].ID,
		OptionID:   poll.Questions[0].Options[0].ID,
	}
	if err := store.SaveResponse(poll.ID, user.ID, "voter-token", []Answer{answer}); err != nil {
		t.Fatalf("save response: %v", err)
	}

	result, err := store.Results(poll.ID)
	if err != nil {
		t.Fatalf("get results: %v", err)
	}
	if result.Total != 1 {
		t.Fatalf("expected total 1, got %d", result.Total)
	}
	if result.Questions[0].Options[0].Count != 1 {
		t.Fatalf("expected first option count 1, got %d", result.Questions[0].Options[0].Count)
	}
}
