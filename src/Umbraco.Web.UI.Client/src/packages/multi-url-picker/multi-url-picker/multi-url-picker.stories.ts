import type { Meta, StoryObj } from '@storybook/web-components';
import './multi-url-picker.element.js';
import type { UmbMultiUrlPickerElement } from './multi-url-picker.element.js';

const meta: Meta<UmbMultiUrlPickerElement> = {
	title: 'Components/Inputs/Multi URL',
	component: 'umb-input-multi-url',
};

export default meta;
type Story = StoryObj<UmbMultiUrlPickerElement>;

export const Overview: Story = {
	args: {},
};
