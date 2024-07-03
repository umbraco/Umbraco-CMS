import type { UmbCurrentUserMfaEnableProviderModalElement } from './current-user-mfa-enable-provider-modal.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './current-user-mfa-enable-provider-modal.element.js';

class UmbServerExtensionsHostElement extends UmbLitElement {
	constructor() {
		super();
		new UmbServerExtensionRegistrator(this, umbExtensionsRegistry).registerAllExtensions();
	}

	override render() {
		return html`<slot></slot>`;
	}
}

if (window.customElements.get('umb-server-extensions-host') === undefined) {
	customElements.define('umb-server-extensions-host', UmbServerExtensionsHostElement);
}

const meta: Meta<UmbCurrentUserMfaEnableProviderModalElement> = {
	title: 'Current User/MFA/Enable MFA Provider',
	component: 'umb-current-user-mfa-enable-provider-modal',
	decorators: [
		(Story) =>
			html`<umb-server-extensions-host style="display: block; width: 500px; height: 500px;">
				${Story()}
			</umb-server-extensions-host>`,
	],
	args: {
		data: {
			providerName: 'SMS',
			displayName: 'SMS',
		},
	},
	parameters: {
		layout: 'centered',
		actions: {
			disabled: true,
		},
	},
};

export default meta;

type Story = StoryObj<UmbCurrentUserMfaEnableProviderModalElement>;

export const Overview: Story = {};
