import { Story, Meta } from '@storybook/web-components';
import { html } from 'lit-html';
import './upgrader.element';

export default {
	title: 'Upgrader/Upgrader',
} as Meta;

const Template: Story = () => html`<umb-upgrader></umb-upgrader>`;

export const Overview = Template.bind({});
