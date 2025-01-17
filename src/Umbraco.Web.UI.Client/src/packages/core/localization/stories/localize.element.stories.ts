import type { UmbLocalizeElement } from '../localize.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';
import '../localize.element.js';

const meta: Meta<UmbLocalizeElement> = {
	title: 'API/Localization/UmbLocalizeElement',
	component: 'umb-localize',
	args: {
		key: 'general_areyousure',
	},
	decorators: [
		(story) => {
			return html`<div style="padding: 1rem; margin: 1rem; border: 1px solid green; max-width:50%;">
				Localized text: "${story()}"
			</div>`;
		},
	],
};

export default meta;

type Story = StoryObj<UmbLocalizeElement>;

export const Default: Story = {
	parameters: {
		docs: {
			source: {
				code: `<umb-localize key="general_areyousure"></umb-localize>`,
			},
		},
	},
};

export const WithArguments: Story = {
	args: {
		key: 'blueprints_createdBlueprintMessage',
		args: ['About us'],
	},
	parameters: {
		docs: {
			source: {
				code: `<umb-localize key="blueprints_createdBlueprintMessage" args="['About us']"></umb-localize>`,
			},
		},
	},
};

export const KeyNotFound: Story = {
	args: {
		key: 'general_ok_not_found',
		debug: true,
	},
	parameters: {
		docs: {
			source: {
				code: `<umb-localize key="general_ok_not_found"></umb-localize>`,
			},
		},
	},
};
