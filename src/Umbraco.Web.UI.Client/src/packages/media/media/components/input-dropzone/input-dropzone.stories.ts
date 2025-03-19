import type { UmbInputDropzoneElement } from './input-dropzone.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';

import './input-dropzone.element.js';

const meta: Meta<UmbInputDropzoneElement> = {
	id: 'umb-input-dropzone',
	title: 'Components/Inputs/Dropzone',
	component: 'umb-input-dropzone',
};

export default meta;

type Story = StoryObj<typeof meta>;

export const Overview: Story = {};
