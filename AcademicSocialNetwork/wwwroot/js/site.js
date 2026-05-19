// ── Photo Preview ──────────────────────────────────────────────────────────
function previewPhoto(input) {
    const container = document.getElementById('photoPreviewContainer');
    const preview   = document.getElementById('photoPreview');
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = e => {
            preview.src = e.target.result;
            container.classList.remove('d-none');
        };
        reader.readAsDataURL(input.files[0]);
    }
}

function removePhoto() {
    document.getElementById('photoInput').value = '';
    document.getElementById('photoPreview').src = '#';
    document.getElementById('photoPreviewContainer').classList.add('d-none');
}

// ── Link Toggle ────────────────────────────────────────────────────────────
function toggleLink() {
    const container = document.getElementById('linkInputContainer');
    const isHidden  = container.classList.contains('d-none');
    container.classList.toggle('d-none', !isHidden);
    if (isHidden) {
        document.getElementById('linkUrlInput').focus();
    } else {
        document.getElementById('linkUrlInput').value = '';
    }
}

function removeLink() {
    document.getElementById('linkUrlInput').value = '';
    document.getElementById('linkInputContainer').classList.add('d-none');
}

document.addEventListener('DOMContentLoaded', () => {
    const token = () => document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    // ── Like (AJAX) ────────────────────────────────────────────────────────
    document.querySelectorAll('.like-btn').forEach(btn => {
        btn.addEventListener('click', async function () {
            const res = await fetch('/Post/LikeAjax', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `postId=${this.dataset.postId}&__RequestVerificationToken=${encodeURIComponent(token())}`
            });
            if (!res.ok) return;

            const data = await res.json();
            this.querySelector('i').className      = data.liked ? 'bi bi-heart-fill' : 'bi bi-heart';
            this.querySelector('span').textContent = data.likeCount;
            this.dataset.liked = data.liked;
        });
    });

    // ── Comment (AJAX) ─────────────────────────────────────────────────────
    document.querySelectorAll('.comment-form').forEach(form => {
        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            const postId  = this.dataset.postId;
            const input   = this.querySelector('input[name="content"]');
            const content = input.value.trim();
            if (!content) return;

            const res = await fetch('/Post/CommentAjax', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `postId=${postId}&content=${encodeURIComponent(content)}&__RequestVerificationToken=${encodeURIComponent(token())}`
            });
            if (!res.ok) return;

            const data = await res.json();

            // Update comment count
            const countBtn = document.querySelector(`[data-bs-target="#comments-${postId}"] span`);
            if (countBtn) countBtn.textContent = `${data.commentCount} Comment${data.commentCount !== 1 ? 's' : ''}`;

            // Append new comment
            const initials = data.authorName.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
            const div = document.createElement('div');
            div.className = 'd-flex gap-2 mb-2';
            div.innerHTML = `
                <div class="avatar-circle avatar-circle-small flex-shrink-0">${initials}</div>
                <div class="bg-light rounded p-2 flex-grow-1">
                    <strong style="color: var(--primary-color); font-size: 0.85rem;">${data.authorName}</strong>
                    <p class="mb-0 small">${data.content}</p>
                    <small class="text-muted">${data.createdAt}</small>
                </div>`;
            this.closest('.pt-3').insertBefore(div, this);
            input.value = '';
        });
    });

    // ── Messages: auto-resize textarea & Enter to send ────────────────────
    const textarea = document.getElementById('messageInput');
    if (textarea) {
        textarea.addEventListener('input', function () {
            this.style.height = 'auto';
            this.style.height = Math.min(this.scrollHeight, 120) + 'px';
        });
        textarea.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.closest('form').submit();
            }
        });
    }

    // ── Messages: user search ──────────────────────────────────────────────
    const searchInput   = document.getElementById('userSearch');
    const searchResults = document.getElementById('userSearchResults');
    const startForm     = document.getElementById('startConvForm');
    const targetInput   = document.getElementById('targetUserId');

    if (searchInput) {
        let debounceTimer;

        searchInput.addEventListener('input', function () {
            clearTimeout(debounceTimer);
            const q = this.value.trim();
            if (q.length < 2) { searchResults.style.display = 'none'; return; }

            debounceTimer = setTimeout(async () => {
                const res   = await fetch(`/Messages/SearchUsers?q=${encodeURIComponent(q)}`);
                const users = await res.json();
                searchResults.innerHTML = '';

                if (!users.length) {
                    searchResults.innerHTML = '<li class="px-3 py-2 text-muted small">No users found.</li>';
                } else {
                    users.forEach(u => {
                        const li      = document.createElement('li');
                        li.className  = 'chat-search-result';
                        li.innerHTML  = `<span class="fw-semibold">${u.fullName}</span><span class="text-muted small ms-2">${u.major ?? ''}</span>`;
                        li.addEventListener('click', () => {
                            targetInput.value = u.id;
                            startForm.submit();
                        });
                        searchResults.appendChild(li);
                    });
                }
                searchResults.style.display = 'block';
            }, 300);
        });

        document.addEventListener('click', e => {
            if (!searchInput.contains(e.target) && !searchResults.contains(e.target))
                searchResults.style.display = 'none';
        });
    }
});
