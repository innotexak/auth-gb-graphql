document.getElementById('mobileMenuBtn').addEventListener('click', function () {
    const menu = document.getElementById('mobileMenu');
    menu.classList.toggle('hidden');
});

// Close menu when a link is clicked
document.querySelectorAll('#mobileMenu a, #mobileMenu button').forEach(link => {
    link.addEventListener('click', function () {
        document.getElementById('mobileMenu').classList.add('hidden');
    });
});

// Desktop dropdown toggle
const desktopDropdownBtn = document.getElementById('desktopDropdownBtn');
const desktopDropdown = document.getElementById('desktopDropdown');

if (desktopDropdownBtn && desktopDropdown) {
    desktopDropdownBtn.addEventListener('click', function (e) {
        e.stopPropagation();
        desktopDropdown.classList.toggle('hidden');
    });

    // Close dropdown when clicking outside
    document.addEventListener('click', function (e) {
        if (!desktopDropdownBtn.contains(e.target) && !desktopDropdown.contains(e.target)) {
            desktopDropdown.classList.add('hidden');
        }
    });

    // Close dropdown when a link is clicked
    desktopDropdown.querySelectorAll('a, button').forEach(item => {
        item.addEventListener('click', function () {
            desktopDropdown.classList.add('hidden');
        });
    });
}