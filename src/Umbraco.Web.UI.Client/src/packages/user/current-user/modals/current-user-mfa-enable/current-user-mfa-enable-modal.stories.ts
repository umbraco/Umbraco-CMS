import type { UmbCurrentUserMfaEnableModalElement } from './current-user-mfa-enable-modal.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import { UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './current-user-mfa-enable-modal.element.js';

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

const meta: Meta<UmbCurrentUserMfaEnableModalElement> = {
	title: 'Current User/MFA/Enable MFA',
	component: 'umb-current-user-mfa-enable-modal',
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

type Story = StoryObj<UmbCurrentUserMfaEnableModalElement>;

export const Overview: Story = {};
