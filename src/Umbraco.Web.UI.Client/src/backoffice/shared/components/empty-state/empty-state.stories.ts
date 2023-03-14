import { Meta, StoryObj } from '@storybook/web-components';
import { html } from 'lit-html';
import './empty-state.element';
import type { UmbEmptyStateElement } from './empty-state.element';

const meta: Meta<UmbEmptyStateElement> = {
    title: 'Components/Empty State',
    component: 'umb-empty-state',
    render: (args) => html`
        <umb-empty-state .position=${args.position} .size=${args.size}>There are no items to be found</umb-empty-state>`,
};
  
export default meta;
type Story = StoryObj<UmbEmptyStateElement>;

export const Overview: Story = {
    args: {
        size: 'large',
    }
};

export const Small: Story = {
    args: {
        size: 'small',
    }
};