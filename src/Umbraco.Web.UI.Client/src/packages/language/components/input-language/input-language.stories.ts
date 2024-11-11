import type { Meta, StoryObj } from '@storybook/web-components';
import './input-language.element.js';
import type { UmbInputLanguageElement } from './input-language.element.js';

const meta: Meta<UmbInputLanguageElement> = {
	title: 'Components/Inputs/Language Picker',
	component: 'umb-input-language-picker',
};

export default meta;
type Story = StoryObj<UmbInputLanguageElement>;

export const Overview: Story = {
	args: {},
};
