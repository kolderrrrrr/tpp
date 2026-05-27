package app

import (
	"crypto/hmac"
	"crypto/rand"
	"crypto/sha256"
	"encoding/base64"
	"fmt"
	"net/http"
	"strconv"
	"strings"
	"time"
)

const sessionCookieName = "polls_session"

func randomToken(size int) string {
	bytes := make([]byte, size)
	if _, err := rand.Read(bytes); err != nil {
		panic(err)
	}
	return base64.RawURLEncoding.EncodeToString(bytes)
}

func hashPassword(password, salt string) string {
	sum := sha256.Sum256([]byte(salt + ":" + password))
	return base64.RawURLEncoding.EncodeToString(sum[:])
}

func secureCompare(a, b string) bool {
	return hmac.Equal([]byte(a), []byte(b))
}

func sign(value string, secret []byte) string {
	mac := hmac.New(sha256.New, secret)
	mac.Write([]byte(value))
	return base64.RawURLEncoding.EncodeToString(mac.Sum(nil))
}

func makeSessionValue(userID int, secret []byte) string {
	value := strconv.Itoa(userID)
	return value + "." + sign(value, secret)
}

func parseSessionValue(raw string, secret []byte) (int, bool) {
	parts := strings.Split(raw, ".")
	if len(parts) != 2 {
		return 0, false
	}
	if !secureCompare(parts[1], sign(parts[0], secret)) {
		return 0, false
	}
	id, err := strconv.Atoi(parts[0])
	return id, err == nil
}

func setSession(w http.ResponseWriter, userID int, secret []byte) {
	http.SetCookie(w, &http.Cookie{
		Name:     sessionCookieName,
		Value:    makeSessionValue(userID, secret),
		Path:     "/",
		HttpOnly: true,
		SameSite: http.SameSiteLaxMode,
		Expires:  time.Now().Add(14 * 24 * time.Hour),
	})
}

func clearSession(w http.ResponseWriter) {
	http.SetCookie(w, &http.Cookie{
		Name:     sessionCookieName,
		Value:    "",
		Path:     "/",
		HttpOnly: true,
		Expires:  time.Unix(0, 0),
		MaxAge:   -1,
	})
}

func voterCookieName(pollID int) string {
	return fmt.Sprintf("poll_%d_voter", pollID)
}
