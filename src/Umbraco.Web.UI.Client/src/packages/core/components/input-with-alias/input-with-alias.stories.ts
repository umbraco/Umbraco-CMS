import type { UmbInputWithAliasElement } from './input-with-alias.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import './input-with-alias.element.js';

const meta: Meta<UmbInputWithAliasElement> = {
	title: 'Generic Components/Inputs/With Alias',
	component: 'umb-input-with-alias',
};

export default meta;
type Story = StoryObj<UmbInputWithAliasElement>;

export const Docs: Story = {
	args: {
		label: 'Input with Alias',
		value: 'Some value',
		alias: 'someAlias',
		required: true,
		autoGenerateAlias: true,
	},
};
