import type { UmbPopoverLayoutElement } from './popover-layout.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './popover-layout.element.js';

const meta: Meta<UmbPopoverLayoutElement> = {
	title: 'Generic Components/Popover/Popover Layout',
	component: 'umb-popover-layout',
	render: () => html`<umb-popover-layout><div>Popover Content</div></umb-popover-layout>`,
};

export default meta;
type Story = StoryObj<UmbPopoverLayoutElement>;

export const Docs: Story = {
	args: {},
};
