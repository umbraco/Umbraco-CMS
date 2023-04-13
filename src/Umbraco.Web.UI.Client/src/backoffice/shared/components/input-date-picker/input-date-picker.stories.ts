import { Meta, StoryObj } from '@storybook/web-components';
import './input-date-picker.element';
import type { UmbInputDatePickerElement } from './input-date-picker.element';

const meta: Meta<UmbInputDatePickerElement> = {
	title: 'Components/Inputs/Date Picker',
	component: 'umb-input-date-picker',
};

export default meta;
type Story = StoryObj<UmbInputDatePickerElement>;

export const Overview: Story = {
	args: {},
};

export const WithOpacity: Story = {
	args: {},
};

export const WithSwatches: Story = {
	args: {},
};
