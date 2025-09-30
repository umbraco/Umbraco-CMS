import type { UmbInputLanguageElement } from './input-language.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './input-language.element.js';

const meta: Meta<UmbInputLanguageElement> = {
	title: 'Entity/Language/Components/Input Language',
	component: 'umb-input-language',
	render: () => html`<umb-input-language></umb-input-language>`,
};

export default meta;
type Story = StoryObj<UmbInputLanguageElement>;

export const Docs: Story = {
	args: {},
};
