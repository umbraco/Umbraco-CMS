import type { UmbCurrentUserExternalLoginModalElement } from './external-login-modal.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

import './external-login-modal.element.js';

class UmbServerExtensionsHostElement extends UmbLitElement {
	constructor() {
		super();
		new UmbServerExtensionRegistrator(this, umbExtensionsRegistry).registerPublicExtensions();
	}

	override render() {
		return html`<slot></slot>`;
	}
}

if (window.customElements.get('umb-server-extensions-host') === undefined) {
	customElements.define('umb-server-extensions-host', UmbServerExtensionsHostElement);
}

const meta: Meta<UmbCurrentUserExternalLoginModalElement> = {
	title: 'Current User/External Login/Configure External Login Providers',
	component: 'umb-current-user-external-login-modal',
	decorators: [
		(Story) =>
			html`<umb-server-extensions-host style="display: block; width: 500px; height: 500px;">
				${Story()}
			</umb-server-extensions-host>`,
	],
	parameters: {
		layout: 'centered',
		actions: {
			disabled: true,
		},
	},
};

export default meta;

type Story = StoryObj<UmbCurrentUserExternalLoginModalElement>;

export const Overview: Story = {};
