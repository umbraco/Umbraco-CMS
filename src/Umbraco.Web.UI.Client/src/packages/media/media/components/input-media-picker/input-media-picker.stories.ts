import { Meta, StoryObj } from '@storybook/web-components';
import './input-media-picker.element.js';
import type { UmbInputMediaPickerElement } from './input-media-picker.element.js';

const meta: Meta<UmbInputMediaPickerElement> = {
	title: 'Components/Inputs/Media Picker',
	component: 'umb-input-media-picker',
};

export default meta;
type Story = StoryObj<UmbInputMediaPickerElement>;

export const Overview: Story = {
	args: {},
};
