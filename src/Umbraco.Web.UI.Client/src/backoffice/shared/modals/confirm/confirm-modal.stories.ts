import './confirm-modal.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';

import type { UmbConfirmModalElement, UmbModalConfirmData } from './confirm-modal.element';

export default {
	title: 'API/Modals/Layouts/Confirm',
	component: 'umb-confirm-modal',
	id: 'umb-confirm-modal',
} as Meta;

const positiveData: UmbModalConfirmData = {
	headline: 'Publish with descendants',
	content: html`Publish <b>This example</b> and all content items underneath and thereby making their content publicly
		available.`,
	confirmLabel: 'Publish',
};

export const Positive: Story<UmbConfirmModalElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-confirm-modal .data=${positiveData as any}></umb-confirm-modal>
`;

const dangerData: UmbModalConfirmData = {
	color: 'danger',
	headline: 'Delete',
	content: html`Delete <b>This example</b> and all items underneath.`,
	confirmLabel: 'Delete',
};

export const Danger: Story<UmbConfirmModalElement> = () => html`
	<!-- TODO: figure out if generics are allowed for properties:
	https://github.com/runem/lit-analyzer/issues/149
	https://github.com/runem/lit-analyzer/issues/163 -->
	<umb-confirm-modal .data=${dangerData as any}></umb-confirm-modal>
`;
