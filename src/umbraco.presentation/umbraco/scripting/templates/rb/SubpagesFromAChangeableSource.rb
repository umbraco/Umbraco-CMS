Umbraco = Object.const_get("umbraco")
Library = Umbraco.const_get("library")
NodeFactory = Umbraco.const_get("presentation").const_get("nodeFactory")

# Set the node id you would like to fetch pages from here
# You can also set it as a macro property with the alias 'nodeId' instead

parent = NodeFactory::Node.new(System::Int32.Parse(nodeId))


# Start writing out the list
result = "<ul>"

parent.Children.each do |child|
  result += "<li><a href='" + Library.NiceUrl(child.Id) + "'>" + child.Name + "</a></li>"
end

result += "</ul>"

puts result