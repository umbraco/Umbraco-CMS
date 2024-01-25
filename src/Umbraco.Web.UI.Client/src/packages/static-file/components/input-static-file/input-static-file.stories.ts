import type { Meta, StoryObj } from '@storybook/web-components';
import './input-static-file.element.js';
import type { UmbInputStaticFileElement } from './input-static-file.element.js';

const meta: Meta<UmbInputStaticFileElement> = {
	title: 'Components/Inputs/Static File',
	component: 'umb-input-static-file',
};

export default meta;
type Story = StoryObj<UmbInputStaticFileElement>;
export const Overview: Story = {
	args: {},
};
