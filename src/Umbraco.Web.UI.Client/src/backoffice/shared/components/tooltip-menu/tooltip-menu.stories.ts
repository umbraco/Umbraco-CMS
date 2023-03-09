import { Meta, StoryObj } from '@storybook/web-components';
import './tooltip-menu.element';
import type { UmbTooltipMenuElement, TooltipMenuItem } from './tooltip-menu.element'

const meta: Meta<UmbTooltipMenuElement> = {
    title: 'Components/Tooltip Menu',
    component: 'umb-tooltip-menu',
};
  
export default meta;
type Story = StoryObj<UmbTooltipMenuElement>;

const items: Array<TooltipMenuItem> = [
    {
        label: 'Item 1',
        icon: 'umb:document',
        action: () => alert('Item 1 clicked'),
    },
    {
        label: 'Item 2',
        icon: 'umb:home',
        action: () => alert('Item 2 clicked')
    }
];

export const Overview: Story = {
    args: {
        items: items,
        iconOnly: false,
    }
};

export const WithIconsOnly: Story = {
    args: {
        items: items,
        iconOnly: true,
    }
};

