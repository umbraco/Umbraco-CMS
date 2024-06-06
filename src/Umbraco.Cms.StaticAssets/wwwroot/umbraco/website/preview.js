!(() => {
  "use strict";

  // Quick exit if within an iframe.
  if (window.self !== window.top) return;

  const styles = `
.umbraco-preview-badge {
    display: inline-flex;
    background: rgba(27, 38, 79, 0.9);
    color: #fff;
    font-size: 87.5%;
    justify-content: center;
    align-items: center;
    box-shadow: 0 5px 10px rgba(0, 0, 0, .2), 0 1px 2px rgba(0, 0, 0, .2);
    line-height: 1;
    pointer-events:none;
    animation: umbraco-preview-badge--effect 10s 1.2s ease both;
    border-radius: 3px 3px 0 0;
}
.umbraco-preview-badge[popover] {
    inset: unset;
    position: fixed;
    bottom: 0;
    left: 50%;
    transform: translate(-50%, 55px);
}
@keyframes umbraco-preview-badge--effect {
    0% { transform: translate(-50%, 55px); animation-timing-function: ease-out; }
    1.5% { transform: translate(-50%, -20px); animation-timing-function: ease-in; }
    5.0% { transform: translate(-50%, -8px); animation-timing-function: ease-in; }
    7.5% { transform: translate(-50%, -4px); animation-timing-function: ease-in; }
    9.2% { transform: translate(-50%, -2px); animation-timing-function: ease-in; }
    3.5%, 6.5%, 8.5% { transform: translate(-50%, 0); animation-timing-function: ease-out; }
    9.7% { transform: translate(-50%, 0); animation-timing-function: ease-out; }
    10.0% { transform: translate(-50%, 0); }
    60% { transform: translate(-50%, 0); animation-timing-function: ease-out; }
    61.5% { transform: translate(-50%, -24px); animation-timing-function: ease-in; }
    65.0% { transform: translate(-50%, -8px); animation-timing-function: ease-in; }
    67.5% { transform: translate(-50%, -4px); animation-timing-function: ease-in; }
    69.2% { transform: translate(-50%, -2px); animation-timing-function: ease-in; }
    63.5%, 66.5%, 68.5% { transform: translate(-50%, 0); animation-timing-function: ease-out; }
    69.7% { transform: translate(-50%, 0); animation-timing-function: ease-out; }
    70.0% { transform: translate(-50%, 0); }
    100.0% { transform: translate(-50%, 0); }
}
.umbraco-preview-badge__header {
    padding: 1em;
    font-weight: bold;
    pointer-events:none;
}
.umbraco-preview-badge__a {
    background: inherit;
    width: 3em;
    padding: 1em;
    display: flex;
    flex-shrink: 0;
    justify-content: center;
    align-items: center;
    align-self: stretch;
    color:white;
    text-decoration:none;
    font-weight: bold;
    border: 0;
    border-left: 1px solid hsla(0,0%,100%,.25);
    pointer-events:all;
    cursor: pointer;
}
.umbraco-preview-badge__a svg {
    width: 1em;
    height:1em;
}
.umbraco-preview-badge__a:hover {
    background: #202d5e00;
}
  `;

  const exitIcon = `
<svg viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg">
  <title>Click to end preview mode</title>
  <path fill="#fff" d="M5273.1 2400.1v-2c0-2.8-5-4-9.7-4s-9.7 1.3-9.7 4v2a7 7 0 002 4.9l5 4.9c.3.3.4.6.4 1v6.4c0 .4.2.7.6.8l2.9.9c.5.1 1-.2 1-.8v-7.2c0-.4.2-.7.4-1l5.1-5a7 7 0 002-4.9zm-9.7-.1c-4.8 0-7.4-1.3-7.5-1.8.1-.5 2.7-1.8 7.5-1.8s7.3 1.3 7.5 1.8c-.2.5-2.7 1.8-7.5 1.8z"/>
  <path fill="#fff" d="M5268.4 2410.3c-.6 0-1 .4-1 1s.4 1 1 1h4.3c.6 0 1-.4 1-1s-.4-1-1-1h-4.3zM5272.7 2413.7h-4.3c-.6 0-1 .4-1 1s.4 1 1 1h4.3c.6 0 1-.4 1-1s-.4-1-1-1zM5272.7 2417h-4.3c-.6 0-1 .4-1 1s.4 1 1 1h4.3c.6 0 1-.4 1-1 0-.5-.4-1-1-1z"/>
  <path fill="#fff" d="M78.2 13l-8.7 11.7a32.5 32.5 0 11-51.9 25.8c0-10.3 4.7-19.7 12.9-25.8L21.8 13a47 47 0 1056.4 0z"/>
  <path fill="#fff" d="M42.7 2.5h14.6v49.4H42.7z"/>
</svg>`;

  class UmbWebsitePreviewElement extends HTMLElement {
    connectedCallback() {
      this.#render();
    }

    async #endPreview() {
      await fetch(`/umbraco/management/api/v1/preview`, {
        method: "DELETE"
      });

      window.location.href = this.getAttribute("url") ?? '/';
    }

    #render() {
      const path = this.getAttribute("path") ?? '/umbraco';
      const unique = this.getAttribute("unique") ?? '';

      const shadow = this.attachShadow({ mode: "open" });

      const wrapper = document.createElement("div");
      wrapper.id = "umbracoPreviewBadge";
      wrapper.className = "umbraco-preview-badge";
      wrapper.popover = "manual";

      const title = document.createElement("span");
      title.className = "umbraco-preview-badge__header";
      title.textContent = "Preview mode";

      wrapper.appendChild(title);

      const btnOpen = document.createElement('a');
      btnOpen.classList.add('umbraco-preview-badge__a', 'open');
      btnOpen.title = 'Open preview in BackOffice';
      btnOpen.href = `${path}/preview/?id=${unique}`;
      btnOpen.innerHTML = '&hellip;';

      wrapper.appendChild(btnOpen);

      const btnExit = document.createElement("button");
      btnExit.type = "button";
      btnExit.classList.add("umbraco-preview-badge__a", "end");
      btnExit.title = "End preview mode";
      btnExit.innerHTML = exitIcon;
      btnExit.onclick = () => this.#endPreview();

      wrapper.appendChild(btnExit);

      const style = document.createElement("style");

      style.textContent = styles;

      shadow.appendChild(style);
      shadow.appendChild(wrapper);
    }
  }

  window.customElements.define("umb-website-preview", UmbWebsitePreviewElement);
})();
