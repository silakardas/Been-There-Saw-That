# 🎬 Been There Saw That

> A personal film diary web app where users can track the movies they've watched, rate them, leave personal notes or favorite quotes, and manage their own profile — all wrapped in a cinematic, film-still-driven UI.

---

## ✨ Features

- **Authentication** — Register and log in with username/password. Includes "remember me" and forgot password flows.
- **User Profiles** — Customizable profiles with a banner image, profile photo, bio, and a pinned favorite film.
- **Film Diary** — Log every movie you've watched with:
  - Poster image upload
  - Title, date watched, genre, director
  - Personal rating (score)
  - A favorite quote or personal note (*Replik / Not*)
- **Film Cards** — Each logged film is displayed as a card with its poster, watch date, director, and your personal note.
- **Edit & Delete** — Update or remove any film entry at any time.
- **Profile Editing** — Change username, name, bio, favorite film, profile photo, and banner image.
- **Account Management** — Option to delete account entirely.



## 📁 Project Structure

```
been-there-saw-that/
├── public/             # Static assets
├── src/
│   ├── components/     # Reusable UI components
│   ├── pages/          # Page-level views (landing, login, register, profile, etc.)
│   ├── services/       # API calls / business logic
│   └── styles/         # Global and component styles
├── server/             # Backend routes, controllers, models
├── .env.example
└── README.md
```

---

## 🌍 Localization

The UI is currently in **Turkish**. Internationalization (i18n) support is planned for future versions.

---

## 🗺️ Roadmap

- [ ] Social features — follow other users, see their diaries
- [ ] Search & filter films by genre, rating, or date
- [ ] Statistics page (films per month, top genres, etc.)
- [ ] Watchlist / "Want to Watch" list
- [ ] Dark / light theme toggle
- [ ] English language support

---

## 🤝 Contributing

Contributions are welcome! Please open an issue first to discuss what you'd like to change.

---

<p align="center">Made with 🎞️ by someone who watches too many movies</p>
