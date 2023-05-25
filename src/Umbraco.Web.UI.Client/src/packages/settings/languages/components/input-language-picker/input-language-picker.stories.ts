import { Meta, StoryObj } from '@storybook/web-components';
import './input-language-picker.element.js';
import type { UmbInputLanguagePickerElement } from './input-language-picker.element.js';

const meta: Meta<UmbInputLanguagePickerElement> = {
	title: 'Components/Inputs/Language Picker',
	component: 'umb-input-language-picker',
};

export default meta;
type Story = StoryObj<UmbInputLanguagePickerElement>;

export const Overview: Story = {
	args: {},
};
