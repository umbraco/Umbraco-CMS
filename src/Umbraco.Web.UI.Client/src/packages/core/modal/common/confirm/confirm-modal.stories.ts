import './confirm-modal.element.js';

import type { UmbConfirmModalElement } from './confirm-modal.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import type { UmbConfirmModalData } from '@umbraco-cms/backoffice/modal';

export default {
	title: 'API/Modals/Layouts/Confirm',
	component: 'umb-confirm-modal',
	id: 'umb-confirm-modal',
} as Meta;

const positiveData: UmbConfirmModalData = {
	headline: 'Publish with descendants',
	content: html`Publish <b>This example</b> and all content items underneath and thereby making their content publicly
		available.`,
	confirmLabel: 'Publish',
};

export const Positive: StoryFn<UmbConfirmModalElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-confirm-modal .data=${positiveData as any}></umb-confirm-modal>
`;

const dangerData: UmbConfirmModalData = {
	color: 'danger',
	headline: 'Delete',
	content: html`Delete <b>This example</b> and all items underneath.`,
	confirmLabel: 'Delete',
};

export const Danger: StoryFn<UmbConfirmModalElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-confirm-modal .data=${dangerData as any}></umb-confirm-modal>
`;
