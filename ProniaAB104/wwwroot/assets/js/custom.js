const cartItemHolder = document.querySelector(".cart-item-holder");
const addToBasketButtons = document.querySelectorAll(".add-to-cart");


addToCartButtons.forEach(button =>
    button.addEventListener("click", ev => {
        ev.preventDefault();

        const href = ev.target.getAttribute("href");

        fetch(href).then(res => res.text()).then(data => {
            cartItemHolder.innerHTML = data;
        })
    })
)