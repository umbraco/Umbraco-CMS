import type { Meta, StoryObj } from '@storybook/web-components';
import './code-block.element.js';
import type { UmbCodeBlockElement } from './code-block.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

const meta: Meta<UmbCodeBlockElement> = {
	title: 'Components/Code Block',
	component: 'umb-code-block',
	parameters: {
		layout: 'centered',
	},
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
