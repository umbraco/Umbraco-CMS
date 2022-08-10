import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit';
import { UmbModalLayoutConfirmElement, UmbModalConfirmData } from './modal-layout-confirm.element';
import './modal-layout-confirm.element';

export default {
	title: 'API/Modals/Layouts/Confirm',
	component: 'umb-modal-layout-confirm',
	id: 'modal-layout-confirm',
} as Meta;

const positiveData: UmbModalConfirmData = {
	headline: 'Publish with descendants',
	content: html`Publish <b>This example</b> and all content items underneath and thereby making their content publicly
		available.`,
	confirmLabel: 'Publish',
};

export const Positive: Story<UmbModalLayoutConfirmElement> = () => html`
	<umb-modal-layout-confirm .data=${positiveData}></umb-modal-layout-confirm>
`;

const dangerData: UmbModalConfirmData = {
	color: 'danger',
	headline: 'Delete',
	content: html`Delete <b>This example</b> and all items underneath.`,
	confirmLabel: 'Delete',
};

export const Danger: Story<UmbModalLayoutConfirmElement> = () => html`
	<umb-modal-layout-confirm .data=${dangerData}></umb-modal-layout-confirm>
`;
