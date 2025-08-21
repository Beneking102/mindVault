using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace MindVault.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public ApiService(HttpClient http, IJSRuntime js)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _js = js ?? throw new ArgumentNullException(nameof(js));
        }

        // Token handling using localStorage via JS Interop
        public async Task SetTokenAsync(string? token)
        {
            if (string.IsNullOrEmpty(token))
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", "mv_token");
                _http.DefaultRequestHeaders.Authorization = null;
            }
            else
            {
                await _js.InvokeVoidAsync("localStorage.setItem", "mv_token", token);
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", "mv_token");
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var res = await _http.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
            if (!res.IsSuccessStatusCode) return false;

            // parse token from JSON response
            using var doc = await JsonDocument.ParseAsync(await res.Content.ReadAsStreamAsync());
            if (doc.RootElement.TryGetProperty("token", out var t) && t.GetString() is string token)
            {
                await SetTokenAsync(token);
                return true;
            }

            return false;
        }

        public async Task RegisterAsync(string email, string password)
        {
            await _http.PostAsJsonAsync("api/auth/register", new { Email = email, Password = password });
        }

        public async Task<List<NoteDto>> GetNotesAsync()
        {
            var notes = await _http.GetFromJsonAsync<List<NoteDto>>("api/notes");
            return notes ?? new List<NoteDto>();
        }

        public async Task<NoteDto?> CreateNoteAsync(NoteDto dto)
        {
            var res = await _http.PostAsJsonAsync("api/notes", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<NoteDto?>();
        }

        // DTO
        public record NoteDto(Guid Id, string UserId, string Title, string Body, DateTime CreatedAt, DateTime UpdatedAt);
    }
}
