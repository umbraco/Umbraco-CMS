import { Meta, StoryObj } from '@storybook/web-components';
import { html } from 'lit';
import type { UmbLocalizeElement } from './localize.element.js';
import './localize.element.js';

const meta: Meta<UmbLocalizeElement> = {
	title: 'Localization/Localize',
	component: 'umb-localize',
	args: {
		key: 'general_ok',
	},
	decorators: [
		(story) => {
			return html`<div style="padding: 1rem; margin: 1rem; border: 1px solid green; max-width:50%;">
				Component output: ${story()}
			</div>`;
		},
	],
	parameters: {
		docs: {
			source: {
				code: `<umb-localize key="general_ok"></umb-localize>`,
			},
		},
	},
};

export default meta;

type Story = StoryObj<UmbLocalizeElement>;

export const Overview: Story = {};
