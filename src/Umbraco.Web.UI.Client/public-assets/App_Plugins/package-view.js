const template = document.createElement('template');
template.innerHTML = `
	<umb-editor-layout role="aside">
		<h1 slot="header">My package view</h1>

		<uui-box>
			<p>Example of vanilla JS section</p>
		</uui-box>

		<uui-action-bar slot="footer">
			<uui-button look="primary" type="button">Close</uui-button>
		</uui-action-bar>
	</umb-editor-layout>
`;

export default class MyPackageViewCustom extends HTMLElement {

	constructor() {
		super();
		this.attachShadow({ mode: 'open' });
		this.shadowRoot.appendChild(template.content.cloneNode(true));

		this.shadowRoot.querySelector('uui-button').addEventListener('click', this.onClick.bind(this));
	}

	onClick() {
		console.log(this.modalHandler);
		this.modalHandler.close();
	}
}

customElements.define('my-package-view-custom', MyPackageViewCustom);
