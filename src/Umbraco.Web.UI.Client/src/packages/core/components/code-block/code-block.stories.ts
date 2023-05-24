import { Meta, StoryObj } from '@storybook/web-components';
import { html } from 'lit';
import './code-block.element';
import type { UmbCodeBlockElement } from './code-block.element.js';

const meta: Meta<UmbCodeBlockElement> = {
	title: 'Components/Code Block',
	component: 'umb-code-block',
};

export default meta;
type Story = StoryObj<UmbCodeBlockElement>;

export const Overview: Story = {
	args: {},
};

export const WithCode: Story = {
	decorators: [],
	render: () => html` <umb-code-block> // Lets write some javascript alert("Hello World"); </umb-code-block>`,
};
