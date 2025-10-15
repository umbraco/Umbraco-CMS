import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbDocumentTreeRepository } from '../../tree/index.js';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbDocumentDetailRepository } from '../../repository/index.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbExportToCsvEntityAction extends UmbEntityActionBase<never> {
  constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
    super(host, args);
  }

  override async execute() {
    if (!this.args.unique) throw new Error('The document unique identifier is missing');

    const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
    const localize = new UmbLocalizationController(this);

    try {
      // Show loading notification
      notificationContext?.peek('default', { 
        data: { message: localize.term('actions_exportingToCsv') } 
      });

      // Get all child items
      const childItems = await this.#getAllChildItems(this.args.unique);
      
      if (childItems.length === 0) {
        notificationContext?.peek('warning', { 
          data: { message: localize.term('actions_noChildrenToExport') } 
        });
        return;
      }

      // Convert to CSV
      const csvContent = this.#convertToCsv(childItems);
      
      // Download the CSV file
      this.#downloadCsv(csvContent, this.args.unique);

      // Show success notification
      notificationContext?.peek('positive', { 
        data: { message: localize.term('actions_exportToCsvSuccess', childItems.length.toString()) } 
      });

    } catch (error) {
      console.error('Export to CSV failed:', error);
      notificationContext?.peek('danger', {       
        data: { message: localize.term('actions_exportToCsvError') } 
      });
    }
  }

  async #getAllChildItems(parentUnique: string): Promise<any[]> {
    const treeRepository = new UmbDocumentTreeRepository(this._host);
    const documentDetailRepository = new UmbDocumentDetailRepository(this._host);
    
    // Get all children recursively
    const allChildren: any[] = [];
    
    const getChildrenRecursively = async (parentId: string) => {
      const { data: children } = await treeRepository.requestTreeItemsOf({
        parent: { unique: parentId, entityType: 'document' },
        skip: 0,
        take: 1000, // Adjust as needed
      });
  
      if (children?.items) {
        for (const child of children.items) {
          try {
            // Get detailed content data with all properties
            const { data: detailData } = await documentDetailRepository.requestByUnique(child.unique);
            
            if (detailData) {
              // Get document type details to access property definitions
              const documentTypeRepository = new UmbDocumentTypeDetailRepository(this._host);
              const { data: documentTypeData } = await documentTypeRepository.requestByUnique(detailData.documentType.unique);
              
              // Combine tree item data with detailed content data
              const combinedData = {
                ...child,
                ...detailData,
                // Add document type with property definitions
                documentType: {
                  ...detailData.documentType,
                  properties: documentTypeData?.properties || []
                },
                // Ensure we have the content values
                values: detailData.values || {}
              };
              
              allChildren.push(combinedData);
            } else {
              // Fallback to tree item data if detail fetch fails
              allChildren.push(child);
            }
          } catch (error) {
            console.warn(`Failed to fetch details for ${child.unique}:`, error);
            // Fallback to tree item data
            allChildren.push(child);
          }
          
          // Recursively get children of this child
          await getChildrenRecursively(child.unique);
        }
      }
    };
  
    await getChildrenRecursively(parentUnique);
    return allChildren;
  }
  #convertToCsv(items: any[]): string {
    if (items.length === 0) return '';

    // Get all unique document type properties from all items
    const allProperties = new Set<string>();
    items.forEach(item => {
        if (item.documentType?.properties) {
        item.documentType.properties.forEach((prop: any) => {
            allProperties.add(prop.alias);
        });
        }
    });
    console.log('Found properties:', Array.from(allProperties));


    // Define CSV headers
    const baseHeaders = [
      'Name',
      'Document Type',
      'Create Date',
      'Update Date',
      'Published',
      'Path',
      'Sort Order',
      'Template',
      'Cultures',
      'Author',
      'Description',
    ];

    // Add document type properties as headers
    const propertyHeaders = Array.from(allProperties).sort();
    const headers = [...baseHeaders, ...propertyHeaders];

    console.log('Final headers:', headers);

    // Convert items to CSV rows
    const rows = items.map(item => {
        const baseRow = [
          this.#escapeCsvValue(item.name || ''),
          this.#escapeCsvValue(item.documentType?.name || ''),
          this.#formatDate(item.createDate),
          this.#formatDate(item.updateDate),
          item.published ? 'Yes' : 'No',
          this.#escapeCsvValue(item.path || ''),
          item.sortOrder?.toString() || '',
          this.#escapeCsvValue(item.template?.name || ''),
          this.#escapeCsvValue(this.#getCultures(item.variants)),
          this.#escapeCsvValue(item.author || ''),
          this.#escapeCsvValue(item.description || ''),
        ];
    
        // Add property values for each document type property
        const propertyValues = propertyHeaders.map(propAlias => {
          const propertyValue = this.#getPropertyValue(item, propAlias);
          return this.#escapeCsvValue(propertyValue);
        });
    
        return [...baseRow, ...propertyValues];
      });

    // Combine headers and rows
    const csvContent = [headers, ...rows]
      .map(row => row.join(','))
      .join('\n');

    return csvContent;
  }

  #escapeCsvValue(value: string): string {
    if (value.includes(',') || value.includes('"') || value.includes('\n')) {
      return `"${value.replace(/"/g, '""')}"`;
    }
    return value;
  }

  #formatDate(dateString: string): string {
    if (!dateString) return '';
    try {
      return new Date(dateString).toLocaleString();
    } catch {
      return dateString;
    }
  }

  #getCultures(variants: any[]): string {
    if (!variants) return '';
    return variants
      .map(v => v.culture)
      .filter(Boolean)
      .join('; ');
  }

  #downloadCsv(csvContent: string, parentUnique: string) {
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = `content-export-${parentUnique}-${Date.now()}.csv`;
    link.style.display = 'none';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    URL.revokeObjectURL(url);
  }
  
    #getPropertyValue(item: any, propertyAlias: string): string {
        let propertyValue = null;
        if (item.values && item.values[propertyAlias]) {
            propertyValue = item.values[propertyAlias];
          }
          else if (item.variants) {
            for (const variant of item.variants) {
              if (variant.values && variant.values[propertyAlias]) {
                propertyValue = variant.values[propertyAlias];
                break;
              }
            }
          }
          else if (item.properties && item.properties[propertyAlias]) {
            propertyValue = item.properties[propertyAlias];
          }
          if (propertyValue === null || propertyValue === undefined) {
            return '';
          }
          if (Array.isArray(propertyValue)) {
            return propertyValue.map(v => {
              if (typeof v === 'object' && v !== null) {
                // Handle media objects
                if (v.mediaKey) {
                  return v.name || v.mediaKey;
                }
                // Handle content picker objects
                if (v.key) {
                  return v.name || v.key;
                }
                // Handle other objects
                return v.name || v.value || JSON.stringify(v);
              }
              return v.toString();
            }).join('; ');
          }
          if (typeof propertyValue === 'object' && propertyValue !== null) {
            // Handle complex objects (like media, content picker, etc.)
            if (propertyValue.name) {
              return propertyValue.name;
            }
            if (propertyValue.value) {
              return propertyValue.value.toString();
            }
            if (propertyValue.text) {
              return propertyValue.text;
            }
            if (propertyValue.mediaKey) {
              return propertyValue.mediaKey;
            }
            if (propertyValue.key) {
              return propertyValue.key;
            }
            return JSON.stringify(propertyValue);
          }
          
          return propertyValue?.toString() || ''
  }
}

export { UmbExportToCsvEntityAction as api };