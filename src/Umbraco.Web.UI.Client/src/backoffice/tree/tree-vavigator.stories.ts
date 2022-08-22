import { Meta, Story } from '@storybook/web-components';
import { LitElement, html } from 'lit';

export default {
	title: 'Tree Navigator',
	component: 'ucp-notification-layout-default',
} as Meta;

const Template: Story = () => html`<umb-tree-navigator></umb-tree-navigator>`;

export const Default = Template.bind({});
