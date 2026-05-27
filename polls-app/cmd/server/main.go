package main

import (
	"log"
	"net/http"
	"os"

	"polls-app/internal/app"
)

func main() {
	addr := getenv("ADDR", ":8080")
	dataFile := getenv("DATA_FILE", "data/store.json")
	sessionSecret := getenv("SESSION_SECRET", "change-me-in-production")

	store, err := app.NewFileStore(dataFile)
	if err != nil {
		log.Fatalf("open store: %v", err)
	}

	server := app.NewServer(store, []byte(sessionSecret))

	log.Printf("Polls app is running at http://%s", displayAddr(addr))
	if err := http.ListenAndServe(addr, server.Routes()); err != nil {
		log.Fatalf("server failed: %v", err)
	}
}

func getenv(key, fallback string) string {
	value := os.Getenv(key)
	if value == "" {
		return fallback
	}
	return value
}

func displayAddr(addr string) string {
	if len(addr) > 0 && addr[0] == ':' {
		return "localhost" + addr
	}
	return addr
}
