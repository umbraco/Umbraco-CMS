import type { UmbDataTypeInputElement } from './data-type-input.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './data-type-input.element.js';

const meta: Meta<UmbDataTypeInputElement> = {
	title: 'Entity/Data Type/Components/Input Data Type',
	component: 'umb-data-type-input',
	render: () => html`<umb-data-type-input></umb-data-type-input>`,
};

export default meta;
type Story = StoryObj<UmbDataTypeInputElement>;

export const Docs: Story = {
	args: {},
};
