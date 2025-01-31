import type { Meta, StoryObj } from '@storybook/web-components';
import './input-block-type.element.js';
import type { UmbInputBlockTypeElement } from './input-block-type.element.js';

const meta: Meta<UmbInputBlockTypeElement> = {
	title: 'Components/Inputs/BlockType',
	component: 'umb-input-media',
};

export default meta;
type Story = StoryObj<UmbInputBlockTypeElement>;

export const Overview: Story = {
	args: {},
};
