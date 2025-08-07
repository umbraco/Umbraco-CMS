import './embedded-media-modal.element.js';
import type UmbEmbeddedMediaModalElement from './embedded-media-modal.element.js';

import type { UmbEmbeddedMediaModalData } from './embedded-media-modal.token.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';

const data: UmbEmbeddedMediaModalData = {
	url: 'https://youtu.be/wJNbtYdr-Hg',
	width: 360,
	height: 240,
	constrain: true,
};

const meta: Meta<UmbEmbeddedMediaModalElement> = {
	title: 'Extension Type/Modal/Embedded Media',
	component: 'umb-embedded-media-modal',
	id: 'umb-embedded-media-modal',
	args: {
		data,
	},
};

export default meta;
type Story = StoryObj<UmbEmbeddedMediaModalElement>;

export const Docs: Story = {};
