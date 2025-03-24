import type { UmbInputDropzoneElement } from './input-dropzone.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './input-dropzone.element.js';

const meta: Meta<UmbInputDropzoneElement> = {
	id: 'umb-input-dropzone',
	title: 'Components/Inputs/Dropzone',
	component: 'umb-input-dropzone',
	args: {
		disabled: false,
		accept: '',
		createAsTemporary: true,
	},
	decorators: [(Story) => html`<div style="width: 300px">${Story()}</div>`],
	parameters: {
		layout: 'centered',
		actions: {
			handles: ['submitted', 'change'],
		},
	},
};

export default meta;

type Story = StoryObj<UmbInputDropzoneElement>;

export const Overview: Story = {};

export const WithDisabled: Story = {
	args: {
		disabled: true,
	},
};

export const WithAccept: Story = {
	args: {
		accept: 'jpg,png',
	},
	parameters: {
		docs: {
			description: {
				story: 'This is a dropzone with an accept attribute set to "jpg,png".',
			},
		},
	},
};

export const WithDefaultSlot: Story = {
	render: () =>
		html`<umb-input-dropzone>
			<div>Custom slot</div>
		</umb-input-dropzone>`,
};
