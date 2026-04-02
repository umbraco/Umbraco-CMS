import type { UmbCodeBlockElement } from './code-block.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './code-block.element.js';

const meta: Meta<UmbCodeBlockElement> = {
	title: 'Generic Components/Code Block',
	component: 'umb-code-block',
};

export default meta;
type Story = StoryObj<UmbCodeBlockElement>;

export const Docs: Story = {
	args: {},
	render: () => html` <umb-code-block> // Lets write some javascript alert("Hello World"); </umb-code-block>`,
};
