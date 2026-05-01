namespace MediaManager.Core.Models;

/// <summary>
/// 操作链模型，定义一系列按顺序执行的媒体操作
/// </summary>
public class OperationChain
{
    /// <summary>
    /// 获取或设置操作链唯一标识符
    /// </summary>
    public string ChainId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 获取或设置操作链名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置操作链描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置操作链版本
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// 获取或设置操作列表（按执行顺序排列）
    /// </summary>
    public List<OperationStep> Steps { get; set; } = new();

    /// <summary>
    /// 获取或设置操作链创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 获取或设置操作链最后修改时间
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// 获取或设置是否在失败时继续执行
    /// </summary>
    public bool ContinueOnFailure { get; set; } = false;

    /// <summary>
    /// 获取或设置操作链执行超时时间（毫秒）
    /// </summary>
    public int TimeoutMs { get; set; } = 300000;

    /// <summary>
    /// 初始化 OperationChain 类的新实例
    /// </summary>
    public OperationChain()
    {
    }

    /// <summary>
    /// 使用指定的名称初始化 OperationChain 类的新实例
    /// </summary>
    /// <param name="name">操作链名称</param>
    public OperationChain(string name)
    {
        Name = name;
    }

    /// <summary>
    /// 添加操作步骤到链尾
    /// </summary>
    /// <param name="step">操作步骤</param>
    public void AddStep(OperationStep step)
    {
        step.StepIndex = Steps.Count;
        Steps.Add(step);
    }

    /// <summary>
    /// 在指定索引位置插入操作步骤
    /// </summary>
    /// <param name="index">插入位置索引</param>
    /// <param name="step">操作步骤</param>
    public void InsertStep(int index, OperationStep step)
    {
        if (index < 0 || index > Steps.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        step.StepIndex = index;
        Steps.Insert(index, step);

        for (int i = index + 1; i < Steps.Count; i++)
        {
            Steps[i].StepIndex = i;
        }
    }

    /// <summary>
    /// 移除指定索引的操作步骤
    /// </summary>
    /// <param name="index">要移除的步骤索引</param>
    public void RemoveStep(int index)
    {
        if (index < 0 || index >= Steps.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        Steps.RemoveAt(index);

        for (int i = index; i < Steps.Count; i++)
        {
            Steps[i].StepIndex = i;
        }
    }

    /// <summary>
    /// 创建操作链的深拷贝
    /// </summary>
    /// <returns>操作链副本</returns>
    public OperationChain Clone()
    {
        return new OperationChain
        {
            ChainId = Guid.NewGuid().ToString(),
            Name = Name,
            Description = Description,
            Version = Version,
            Steps = Steps.Select(s => s.Clone()).ToList(),
            ContinueOnFailure = ContinueOnFailure,
            TimeoutMs = TimeoutMs
        };
    }
}

/// <summary>
/// 操作步骤模型，定义单个媒体操作的详细信息
/// </summary>
public class OperationStep
{
    /// <summary>
    /// 获取或设置步骤唯一标识符
    /// </summary>
    public string StepId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 获取或设置步骤索引（在操作链中的位置）
    /// </summary>
    public int StepIndex { get; set; }

    /// <summary>
    /// 获取或设置步骤名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置步骤描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置要执行的组件类型名称
    /// </summary>
    public string ComponentTypeName { get; set; } = string.Empty;

    /// <summary>
    /// 获取或设置组件参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// 获取或设置是否启用此步骤
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 获取或设置步骤执行超时时间（毫秒）
    /// </summary>
    public int? StepTimeoutMs { get; set; }

    /// <summary>
    /// 初始化 OperationStep 类的新实例
    /// </summary>
    public OperationStep()
    {
    }

    /// <summary>
    /// 使用指定的名称和组件类型初始化 OperationStep 类的新实例
    /// </summary>
    /// <param name="name">步骤名称</param>
    /// <param name="componentTypeName">组件类型名称</param>
    public OperationStep(string name, string componentTypeName)
    {
        Name = name;
        ComponentTypeName = componentTypeName;
    }

    /// <summary>
    /// 创建操作步骤的深拷贝
    /// </summary>
    /// <returns>操作步骤副本</returns>
    public OperationStep Clone()
    {
        return new OperationStep
        {
            StepId = Guid.NewGuid().ToString(),
            StepIndex = StepIndex,
            Name = Name,
            Description = Description,
            ComponentTypeName = ComponentTypeName,
            Parameters = new Dictionary<string, object>(Parameters),
            IsEnabled = IsEnabled,
            StepTimeoutMs = StepTimeoutMs
        };
    }
}
