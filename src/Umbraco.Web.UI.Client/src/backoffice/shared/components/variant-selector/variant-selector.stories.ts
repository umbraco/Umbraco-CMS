import { Meta, StoryObj } from '@storybook/web-components';
import './variant-selector.element';
import type { UmbVariantSelectorElement } from './variant-selector.element'

const meta: Meta<UmbVariantSelectorElement> = {
    title: 'Components/Variant Selector',
    component: 'umb-variant-selector',
};
  
export default meta;
type Story = StoryObj<UmbVariantSelectorElement>;

export const Overview: Story = {
    args: {
        alias: 'myAlias'   
    }
};