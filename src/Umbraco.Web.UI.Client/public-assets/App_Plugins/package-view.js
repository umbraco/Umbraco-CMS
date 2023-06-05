const template = document.createElement('template');
template.innerHTML = `
	<umb-body-layout role="aside">
		<h1 slot="header">My package view</h1>

		<uui-box>
			<p>Example of vanilla JS section</p>
		</uui-box>

		<uui-action-bar slot="footer-info">
			<uui-button look="primary" type="button">Close</uui-button>
		</uui-action-bar>
	</umb-body-layout>
`;

export default class MyPackageViewCustom extends HTMLElement {
	constructor() {
		super();
		this.attachShadow({ mode: 'open' });
		this.shadowRoot.appendChild(template.content.cloneNode(true));

		this.shadowRoot.querySelector('uui-button').addEventListener('click', this.onClick.bind(this));
	}

	onClick() {
		this.modalContext.close();
	}
}

customElements.define('my-package-view-custom', MyPackageViewCustom);
