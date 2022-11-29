let navbar = document.getElementById('login-nav');

const getLoginPreviewUrl = `${window.location.origin}/Account/UserLoginPreview`;

fetch(getLoginPreviewUrl)
    .then(function (response) {
        return response.text();
    }).then(function (result) {
        navbar.innerHTML = result;
    }).catch(function () {
        console.error('smth goes wrong');
    });
