import type { UmbStylesheetInputElement } from './stylesheet-input.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './stylesheet-input.element.js';

const meta: Meta<UmbStylesheetInputElement> = {
	title: 'Entity/Stylesheet/Components/Input Stylesheet',
	component: 'umb-stylesheet-input',
	render: () => html`<umb-stylesheet-input></umb-stylesheet-input>`,
};

export default meta;
type Story = StoryObj<UmbStylesheetInputElement>;

export const Docs: Story = {
	args: {},
};
