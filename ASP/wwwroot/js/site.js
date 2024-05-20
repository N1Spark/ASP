document.addEventListener('submit', e => {
    console.log(e);
    const form = e.target;
    if (form.id == 'room-form')// перехоплюємо надсилання форми і переводимо до аякс
    {
        e.preventDefault();
        let formData = new FormData(form);
        if (!document.querySelector('[name = "room-name"]').value) {
            alert("Поле имени должен быть заполнен");
            return
        }
        if (!document.querySelector('[name = "room-description"]').value) {
            alert("Поле описания должен быть заполнен");
            return
        }
        if (!document.querySelector('[name = "room-slug"]').value) {
            alert("Поле slug должен быть заполнен");
            return
        }
        if (!document.querySelector('[name = "room-stars"]').value) {
            alert("Поле оценки должен быть заполнен");
            return
        }
        if (!document.querySelector('[name = "room-price"]').value) {
            alert("Поле цены должен быть заполнен");
            return
        }
        fetch("/api/room", {
            method: 'POST',
            body: formData
        }).then(r => {
            console.log(r);
            if (r.status == 201) {
                window.location.reload();
            }
        });
    }
    if (form.id == 'category-form') {
        e.preventDefault();
        let formData = new FormData(form);
        // перевіряємо чи це додавання чи редагування.
        // Ознака - наявність поля category-id
        ctgId = formData.get("category-id");
        if (ctgId) {
            // оновлення
            console.log("Оновлення категорії " + ctgId);
            fetch('/api/category', {
                method: 'PUT',
                body: formData
            }).then(r => {
                if (r.status < 300) {
                    window.location.reload();
                }
                else {
                    r.text().then(alert);
                }
            });
        }
        else {
            // додавання (створення)
            console.log("Додавання нової категорії");
            fetch('/api/category', {
                method: 'POST',
                body: formData
            }).then(r => {
                if (r.status == 201) {
                    window.location.reload();
                }
                else {
                    r.text().then(alert);
                }
            });
        }
    }
    // на інші форми ми не вплюваємо
});
document.addEventListener('DOMContentLoaded', function () {
    const authButton = document.getElementById("auth-button");
    if (authButton) authButton.addEventListener('click', authButtonClick);

    const confirmEmailButton = document.getElementById("confirm-email-button");
    if (confirmEmailButton) confirmEmailButton.addEventListener('click', confirmEmailClick);

    const confirmPasswordButton = document.getElementById("confirm-password-button");
    if (confirmPasswordButton) confirmPasswordButton.addEventListener('click', restoreUserPassword);

    initAdminPage();
    serveReserveButtons();
    serveAdminButtons();
});

function serveAdminButtons() {
    for (let btn of document.querySelectorAll('[data-type="edit-category"]')) {
        btn.addEventListener('click', e => {
            let b = e.target.closest('[data-type="edit-category"]');
            document.querySelector('[name = "category-id"]').value =
                b.getAttribute("data-category-id");
            document.querySelector('[name = "category-name"]').value =
                b.getAttribute("data-category-name");
            document.querySelector('[name = "category-description"]').value =
                b.getAttribute("data-category-description");
            document.querySelector('[name = "category-slug"]').value =
                b.getAttribute("data-category-slug");
        });
    }
    for (let btn of document.querySelectorAll('[data-type="delete-category"]')) {
        btn.addEventListener("click", e => {
            let b = e.target.closest('[data-type="delete-category"]');
            let id = b.getAttribute("data-category-id");
            if (id) {
                if (confirm("Ви підтверджуєте видалення категорії?")) {
                    fetch(`/api/category/${id}`, { method: 'DELETE' }).then(r => {
                        if (r.status < 400) {
                            window.location.reload();
                        }
                        else {
                            alert("Виникла помилка видалення");
                        }
                    })
                }
            }
            else {
                alert("Помилка розмітки - немає id елемента");
            }
        });
    }

    for (let btn of document.querySelectorAll('[data-type="restore-category"]')) {
        btn.addEventListener("click", e => {
            let b = e.target.closest('[data-type="restore-category"]');
            let id = b.getAttribute("data-category-id");
            if (id) {
                if (confirm("Ви підтверджуєте вiдновлення категорії?")) {
                    fetch(`/api/category?id=${id}`, { method: 'RESTORE' }).then(r => {
                        if (r.status < 400) {
                            window.location.reload();
                            //r.text().then(console.log);
                        }
                        else {
                            alert("Виникла помилка вiдновлення");
                        }
                    })
                }
            }
            else {
                alert("Помилка розмітки - немає id елемента");
            }
        });
    }
}

function authButtonClick() {
    const authEmail = document.getElementById("auth-email");
    if (!authEmail) throw "Element '#authEmail' not found!"
    const authPassword = document.getElementById("auth-password");
    if (!authPassword) throw "Element '#authPassword' not found!"
    const authMessage = document.getElementById("auth-message");
    if (!authMessage) throw "Element '#authMessage' not found!"

    const email = authEmail.value.trim();
    if (!email) {
        authMessage.classList.remove('visually-hidden');
        authMessage.innerText = "Hеобхідно ввести E-mail";
        return;
    }
    const password = authPassword.value;

    fetch(`/api/auth?e-mail=${email}&password=${password}`)
        .then(r => {
            if (r.status != 200) {
                authMessage.classList.remove('visually-hidden');
                authMessage.innerText = "Вхід скасовано, перевірте введені дані";
            }
            else {
                // За вимогами безпеки зміна стагусу авторизації потребує перезавантаження
                window.location.reload();
            }
        });
}

function confirmEmailClick() {
    const emailCodeInput = document.getElementById("email-code");
    if (!emailCodeInput) throw "Element '#email-code' not found!"
    const emailhMessage = document.getElementById("email-message");
    if (!emailhMessage) throw "Element '#email-message' not found!"
    const code = emailCodeInput.value.trim();
    if (!code) {
        emailhMessage.classList.remove('visually-hidden');
        emailhMessage.innerText = "Hеобхідно ввести код";
        return;
    }
    const email = emailCodeInput.getAttribute("data-email");

    fetch(`/api/auth?email=${email}&code=${code}`, { method: 'PATCH' })
        .then(r => {
            if (r.status === 202) {
                window.location.reload();
            }
            else {
                emailhMessage.classList.remove('visually-hidden');
                emailhMessage.innerText = "Код не прийнято"
            }
        });
}

function restoreUserPassword() {
    const emailInput = document.getElementById("password-email");
    if (!emailInput) throw "Element '#password-email' not found!"

    const passNameInput = document.getElementById("password-name");
    if (!passNameInput) throw "Element '#password-name' not found!"

    const passwordMessage = document.getElementById("password-message-error");
    if (!passwordMessage) throw "Element '#password-message-error' not found!"

    const passwordMessageSuccess = document.getElementById("password-message-success");
    if (!passwordMessageSuccess) throw "Element '#password-message-success' not found!"

    if (!emailInput.value) {
        passwordMessage.classList.remove('visually-hidden');
        passwordMessageSuccess.classList.add('visually-hidden');
        passwordMessage.innerText = "Hеобхідно ввести Email";
        return;
    }
    if (!passNameInput.value) {
        passwordMessage.classList.remove('visually-hidden');
        passwordMessageSuccess.classList.add('visually-hidden');
        passwordMessage.innerText = "Hеобхідно ввести UserName";
        return;
    }

    fetch(`/api/auth?email=${emailInput.value}&username=${passNameInput.value}`, { method: 'RESTORE' })
        .then(r => {
            if (r.status === 202) {
                passwordMessage.classList.add('visually-hidden');
                passwordMessageSuccess.remove('visually-hidden');
            }
            else {
                passwordMessage.classList.remove('visually-hidden');
                passwordMessage.innerText = "Не удалось восстановить пароль"
            }
        });
}


///// ADMIN PAGE //////
function initAdminPage() {
    loadCategories();
}
function loadCategories() {
    const container = document.getElementById("category-container");
    if (!container) return;
    fetch("/api/category")      // запитуємо наявні категорії
        .then(r => r.json())
        .then(j => {
            let html = "";
            for (let ctg of j) {
                html += `<p data-id="${ctg["id"]}" onclick="ctgClick('${ctg["id"]}')">${ctg["name"]}</p>`;
            }
            html += `Назва: <input id="ctg-name" /><br/>
            Опис: <textarea id="ctg-description"></textarea><br/>
            Фото: <input type="file" id="ctg-photo" /><br/>
            <button onclick='addCategory()'>+</button>`;
            container.innerHTML = html;
        });

}
function ctgClick(ctgid) {
    //const ctgid = e.target.closest('[data-id]').getAttribute('data-id');
    fetch("/api/location/" + ctgid)
        .then(r => r.json())
        .then(j => {
            const container = document.getElementById("location-container");
            let html = "";
            for (let loc of j) {
                html += `<p data-id="${loc["id"]}" onclick="locClick(event)">${loc["name"]}</p>`;
            }
            html += `Назва: <input id="loc-name" /><br/>
            Опис: <textarea id="loc-description"></textarea><br/>
            Рейтинг: <input id="loc-stars" type="number"/><br/>
            Фото: <input type="file" id="loc-photo" /><br/>
            <button onclick='addLocation("${ctgid}")'>+</button>`;
            container.innerHTML = html;
        });
}

function addCategory() {
    const ctgName = document.getElementById("ctg-name").value;
    const ctgDescription = document.getElementById("ctg-description").value;
    const ctgPhoto = document.getElementById("ctg-photo");

    if (confirm(`Додаємо категорію ${ctgName} ${ctgDescription} ?`)) {
        let formData = new FormData();
        formData.append("name", ctgName);
        formData.append("description", ctgDescription);
        formData.append("photo", ctgPhoto.files[0]);

        fetch("/api/category", {
            method: 'POST',
            body: formData
        })
            .then(r => {
                if (r.status == 201) {
                    loadCategories();
                }
                else {
                    alert("error");
                }
            });
    }
}


function addLocation(ctgid) {
    const ctgName = document.getElementById("loc-name").value;
    const ctgDescription = document.getElementById("loc-description").value;
    const ctgStars = document.getElementById("loc-stars").value;
    const locPhoto = document.getElementById("loc-photo");
    if (confirm(`Додаємо локацiю ${ctgName} ${ctgDescription} ${ctgStars} ?`)) {
        let formData = new FormData();
        formData.append("categoryId", ctgid);
        formData.append("name", ctgName);
        formData.append("description", ctgDescription);
        formData.append("stars", ctgStars);
        formData.append("photo", locPhoto.files[0]);
        fetch("/api/location", {
            method: 'POST',
            body: formData
        })
            .then(r => {
                if (r.status == 201) {
                    ctgClick(ctgid);
                }
                else {
                    alert("error");
                }
            });
    }
}

function serveReserveButtons() {
    for (let btn of document.querySelectorAll('[data-type="reserve-room"]')) {
        btn.addEventListener('click', e => {
            const cont = e.target.closest('[data-type="reserve-room"]');
            const roomId = cont.getAttribute('data-room-id');
            const userId = cont.getAttribute('data-user-id');
            const roomName = cont.getAttribute('data-room-name');
            const date = cont.getAttribute('data-date');

            console.log(roomId, userId, date);
            let userChoice = confirm("Вы действительно хотите забронировать номер: " + roomName + " на дату: " + date + "?");
            if (userChoice) {
                fetch('/api/room/reserve', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        date,
                        roomId,
                        userId
                    })
                }).then(r => {
                    if (r.status === 201) {
                        window.location.reload();
                    }
                    else {
                        r.text().then(alert);
                    }
                });
            }
        });
    }

    for (let btn of document.querySelectorAll('[data-type="drop-reserve-room"]')) {
        btn.addEventListener('click', e => {
            const cont = e.target.closest('[data-type="drop-reserve-room"]');
            const reserveId = cont.getAttribute('data-reserve-id');
            const roomName = cont.getAttribute('data-room-name');
            const date = cont.getAttribute('data-date');

            if (!confirm("Вы действительно хотите отменить заказ номера: " + roomName + " на дату: " + date + "?")) {
                return;
            }
            fetch('/api/room/reserve?reserveId=' + reserveId, {
                method: 'DELETE',
            }).then(r => {
                if (r.status === 202) {
                    window.location.reload();
                }
                else {
                    r.text().then(alert);
                }
            });
        });
    }
}

/* CRUD
  Delete: видалення буває "м'яким" та "жорстким"
  жорстке Chard) - реальне видалення як правило без можливості відновлення
  м'яке - помітка даних як "видалені" 

*/