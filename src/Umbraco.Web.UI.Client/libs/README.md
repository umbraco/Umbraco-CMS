# @umbraco-cms/backoffice

This package contains the types for the libraries for Umbraco 13.

## Installation

```bash
npm install -D @umbraco-cms/backoffice
```

## Usage

### Vanilla JavaScript

Create an umbraco-package.json file in the root of your package.

```json
{
	"name": "My.Package",
	"version": "0.1.0",
	"extensions": [
		{
			"type": "dashboard",
			"alias": "my.custom.dashboard",
			"name": "My Dashboard",
			"js": "/App_Plugins/MyPackage/dashboard.js",
			"weight": -1,
			"meta": {
				"label": "My Dashboard",
				"pathname": "my-dashboard"
			},
			"conditions": {
				"sections": ["Umb.Section.Content"]
			}
		}
	]
}
```

Then create a dashboard.js file the same folder.

```javascript
import { UmbElementMixin } from '@umbraco-cms/backoffice/element';
import { UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

const template = document.createElement('template');
template.innerHTML = `
  <style>
    :host {
      padding: 20px;
      display: block;
      box-sizing: border-box;
    }
  </style>

  <uui-box>
    <h1>Welcome to my dashboard</h1>
    <p>Example of vanilla JS code</p>

    <uui-button label="Click me" id="clickMe" look="secondary"></uui-button>
  </uui-box>
`;

export default class MyDashboardElement extends UmbElementMixin(HTMLElement) {
	/** @type {import('@umbraco-cms/backoffice/notification').UmbNotificationContext} */
	#notificationContext;

	constructor() {
		super();
		this.attachShadow({ mode: 'open' });
		this.shadowRoot.appendChild(template.content.cloneNode(true));

		this.shadowRoot.getElementById('clickMe').addEventListener('click', this.onClick.bind(this));

		this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (_instance) => {
			this.#notificationContext = _instance;
		});
	}

	onClick = () => {
		this.#notificationContext?.peek('positive', { data: { headline: 'Hello' } });
	};
}

customElements.define('my-custom-dashboard', MyDashboardElement);
```

### TypeScript with Lit

First install Lit and Vite. This command will create a new folder called `my-package` which will have the Vite tooling and Lit for WebComponent development setup.

```bash
npm create vite@latest -- --template lit-ts my-package
```

Go to the new folder and install the backoffice package.

```bash
cd my-package
npm install -D @umbraco-cms/backoffice
```

Then go to the element located in `src/my-element.ts` and replace it with the following code.

```typescript
// src/my-element.ts
import { LitElement, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';

@customElement('my-element')
export default class MyElement extends UmbElementMixin(LitElement) {

	private _notificationContext?: UmbNotificationContext;

	constructor() {
		super();
		this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (_instance) => {
			this._notificationContext = _instance;
		});
	}

	onClick() {
		this._notificationContext?.peek('positive', { data: { message: '#h5yr' } });
	}

	render() {
		return html`
			<uui-box headline="Welcome">
				<p>A TypeScript Lit Dashboard</p>
				<uui-button look="primary" label="Click me" @click=${() => this.onClick()}></uui-button>
			</uui-box>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'my-element': MyElement;
	}
}
```

Finally add an umbraco-package.json file in the root of your package folder `my-package`.

```json
{
	"name": "My.Package",
	"version": "0.1.0",
	"extensions": [
		{
			"type": "dashboard",
			"alias": "my.custom.dashboard",
			"name": "My Dashboard",
			"js": "/App_Plugins/MyPackage/dist/my-package.js",
			"weight": -1,
			"meta": {
				"label": "My Dashboard",
				"pathname": "my-dashboard"
			},
			"conditions": {
				"sections": ["Umb.Section.Content"]
			}
		}
	]
}
```
