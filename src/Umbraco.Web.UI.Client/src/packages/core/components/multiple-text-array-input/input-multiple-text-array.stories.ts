import type { UmbInputMultipleTextArrayElement } from './input-multiple-text-array.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './input-multiple-text-array.element.js';

const meta: Meta<UmbInputMultipleTextArrayElement> = {
	title: 'Generic Components/Inputs/Multiple Text Array',
	component: 'umb-input-multiple-text-array',
	render: (args) =>
		html`<umb-input-multiple-text-array
			.min="${args.min}"
			.max="${args.max}"
			.minMessage="${args.minMessage}"
			.maxMessage="${args.maxMessage}"></umb-input-multiple-text-array>`,
};

export default meta;
type Story = StoryObj<UmbInputMultipleTextArrayElement>;

export const Docs: Story = {
	args: {
		min: 0,
		max: 10,
		minMessage: 'This field needs more items',
		maxMessage: 'This field has too many items',
	},
};
