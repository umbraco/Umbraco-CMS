import { Meta, StoryObj } from '@storybook/web-components';
import { html } from 'lit';
import './template-card.element';
import type { UmbTemplateCardElement } from './template-card.element';

const meta: Meta<UmbTemplateCardElement> = {
	title: 'Components/Template Card',
	component: 'umb-template-card',
};

export default meta;
type Story = StoryObj<UmbTemplateCardElement>;

export const Overview: Story = {
	args: {
		name: 'Template with a name ',
	},
};

export const Default: Story = {
	args: {
		name: 'Just a template',
	},
};

export const LongName: Story = {
	args: {
		name: 'Another template that someone gave a way way too long name without really thinking twice about it',
	},
};

export const TemplateCardList: Story = {
	render: () => html`<div style="display:flex;align-items:stretch; gap:10px; padding:10px">
		<umb-template-card name="Template with a name" default="true"></umb-template-card>
		<umb-template-card
			name="Another template that someone gave a way way too long name without really thinking twice about it"></umb-template-card>
		<umb-template-card name="Another template"></umb-template-card>
		<umb-template-card name="Templates really shouldn't have such long names"></umb-template-card>
	</div>`,
};
