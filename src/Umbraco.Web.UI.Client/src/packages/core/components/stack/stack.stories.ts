import type { UmbStackElement } from './stack.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './stack.element.js';

const meta: Meta<UmbStackElement> = {
	title: 'Generic Components/Stack',
	component: 'umb-stack',
	render: (args) =>
		html`<umb-stack .divide=${args.divide} .look=${args.look}>
			<div>Element 1</div>
			<div>Element 2</div>
			<div>Element 3</div>
		</umb-stack>`,
};

export default meta;
type Story = StoryObj<UmbStackElement>;

export const Divide: Story = {
	args: {
		divide: true,
	},
};

export const Compact: Story = {
	args: {
		look: 'compact',
	},
};
