import sys
import uuid
import json
import base64
import asyncio
import httpx

CRUNCHY_BASE = "https://beta-api.crunchyroll.com"
CRUNCHY_AUTH = "eHVuaWh2ZWRidDNtYmlzdWhldnQ6MWtJUzVkeVR2akUwX3JxYUEzWWVBaDBiVVhVbXhXMTE="


async def login(username: str, password: str) -> str:
    """Login to Crunchyroll and return access token."""
    device_id = str(uuid.uuid4())
    headers = {
        "Authorization": f"Basic {CRUNCHY_AUTH}",
        "ETP-Anonymous-ID": str(uuid.uuid4()),
        "Content-Type": "application/x-www-form-urlencoded",
    }

    body = {
        "scope": "offline_access",
        "device_name": "PythonScript",
        "device_id": device_id,
        "device_type": "com.example.device",
        "grant_type": "password",
        "username": username,
        "password": password,
    }

    async with httpx.AsyncClient(timeout=20) as client:
        resp = await client.post(f"{CRUNCHY_BASE}/auth/v1/token", headers=headers, data=body)
        resp.raise_for_status()
        return resp.json()["access_token"]


async def fetch_anime_list(token: str, limit: int = 200) -> list:
    """Fetch anime list with metadata."""
    url = f"{CRUNCHY_BASE}/content/v2/discover/browse"
    headers = {"Authorization": f"Bearer {token}"}
    params = {"start": 0, "n": limit, "sort_by": "alphabetical", "sort_order": "asc", "locale": "en-US"}

    async with httpx.AsyncClient(timeout=30) as client:
        resp = await client.get(url, headers=headers, params=params)
        resp.raise_for_status()
        data = resp.json()

    shows = data.get("items") or data.get("data") or []
    results = []

    async with httpx.AsyncClient(timeout=30) as client:
        for show in shows:
            anime_id = show.get("id")
            title = show.get("title")
            desc = show.get("description") or ""
            ratings = (show.get("series_metadata") or {}).get("maturity_ratings", [])
            episodes = (show.get("series_metadata") or {}).get("episode_count")
            seasons = (show.get("series_metadata") or {}).get("season_count")
            genres = (show.get("series_metadata") or {}).get("tenant_categories", [])
            descriptors = (show.get("series_metadata") or {}).get("content_descriptors", [])
            pub_date = (show.get("series_metadata") or {}).get("series_launch_year")

            # Fetch and convert poster image to base64
            img_url = None
            img_b64 = None
            if show.get("images") and "poster_wide" in show["images"]:
                img_url = show["images"]["poster_wide"][0][0]["source"]
                try:
                    r = await client.get(img_url)
                    if r.status_code == 200:
                        img_b64 = base64.b64encode(r.content).decode("utf-8")
                except Exception:
                    pass

            results.append({
                "id": anime_id,
                "title": title,
                "description": desc,
                "ratings": ratings,
                "episodes": episodes,
                "seasons": seasons,
                "genres": genres,
                "descriptors": descriptors,
                "publish_date": pub_date,
                "image_base64": img_b64,
            })

    return results


async def main():
    if len(sys.argv) < 3:
        print(f"Usage: python {sys.argv[0]} <username> <password>")
        sys.exit(1)

    username, password = sys.argv[1], sys.argv[2]

    print("Logging in...")
    token = await login(username, password)
    print("Fetching anime list...")
    anime_list = await fetch_anime_list(token, limit=2000)

    with open("animes.json", "w", encoding="utf-8") as f:
        json.dump(anime_list, f, ensure_ascii=False, indent=2)

    print(f"Saved {len(anime_list)} animes to animes.json")


if __name__ == "__main__":
    asyncio.run(main())
