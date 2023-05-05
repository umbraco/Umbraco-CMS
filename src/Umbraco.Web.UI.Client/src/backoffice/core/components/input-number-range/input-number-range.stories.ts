import { Meta, StoryObj } from '@storybook/web-components';
import './input-number-range.element';
import type { UmbInputNumberRangeElement } from './input-number-range.element';

const meta: Meta<UmbInputNumberRangeElement> = {
    title: 'Components/Inputs/Number Range Picker',
    component: 'umb-input-number-range'
};
  
export default meta;
type Story = StoryObj<UmbInputNumberRangeElement>;
  
export const Overview: Story = {
    args: {  
    }
};

export const WithMinMax: Story = {
    args: {  
        minValue:0,
        maxValue:40,
    }
};
