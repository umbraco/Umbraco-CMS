import './confirm-modal.element.js';

import type { UmbConfirmModalElement } from './confirm-modal.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import type { UmbConfirmModalData } from '@umbraco-cms/backoffice/modal';

const meta: Meta<UmbConfirmModalElement> = {
	title: 'Extension Type/Modal/Confirm',
	component: 'umb-confirm-modal',
	id: 'umb-confirm-modal',
	args: {
		data: {
			headline: '[Headline]',
			content: html`[Content]`,
			color: 'positive',
			cancelLabel: '[Cancel button]',
			confirmLabel: '[Confirm button]',
		},
	},
} as Meta;

export default meta;
type Story = StoryObj<UmbConfirmModalElement>;

const positiveData: UmbConfirmModalData = {
	headline: 'Publish with descendants',
	content: html`Publish <b>This example</b> and all content items underneath and thereby making their content publicly
		available.`,
	confirmLabel: 'Publish',
};

export const Positive: Story = {
	args: {
		data: positiveData,
	},
};

const dangerData: UmbConfirmModalData = {
	color: 'danger',
	headline: 'Delete',
	content: html`Delete <b>This example</b> and all items underneath.`,
	confirmLabel: 'Delete',
};

export const Danger: Story = {
	args: {
		data: dangerData,
	},
};
